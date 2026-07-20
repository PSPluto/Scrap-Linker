using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class BaseScrap : MonoBehaviour
{

    public enum ScrapState
    {
        // ロープに従っている状態
        Tethered,
        // プレイヤーに持ち上げられている状態
        Lifted,
        // 投げられている状態
        InFlight,
        // 通常状態
        Usually
    }

    [Header ("=== Rigidbody ===")]
        [SerializeField] private Rigidbody myRb;
        [Tooltip("摩擦")]public float damp = 2;
        [Tooltip("質量")]public float mass = 1f; 

    [Header ("=== Scrapの設定 ===")]

    [Header("・弱ヒット")]
        [Tooltip("弱 ダメージ")] public float weakDamage;
        [HideInInspector][Tooltip("弱ヒットの閾値 (以下)")] public float weakHitThreshold = 5;
        [Tooltip("弱ヒットの音")] public AudioClip weakHitSound;

    [Header("・基本ヒット")]   
        [Tooltip("基本 ダメージ")] public float baseDamage;
        [Tooltip("基本ヒットの音")] public AudioClip baseHitSound;


    [FormerlySerializedAs("CriticalDamage")]
    [Header("・強ヒット")]
        [Tooltip("クリティカル ダメージ")] public float criticalDamage;
        [HideInInspector][Tooltip("強ヒットの閾値 (以上)")] public float criticalHitThreshold = 8;
        [Tooltip("強ヒットの音")] public AudioClip criticalHitSound;  

    [Header("・ダメージ0")]
        [Tooltip("ダメージ0の音")] public AudioClip noDamageSound;
    [HideInInspector][Tooltip("Zeroヒットの閾値 (以下)")] public float noDamageHitThreshold = 3;


    [Header("=== State ===")]
    [Tooltip("Scrapの状態")] public ScrapState scrapState = ScrapState.Usually;
    private ScrapState _lastState = ScrapState.Usually;

    [Tooltip("まとまった状態の1要素か")] public bool isMerged;
    private bool _isMergedLast;

    private void OnCollisionEnter(Collision collision)
    {

        if (scrapState == ScrapState.InFlight)
        {
            scrapState = ScrapState.Usually;
        }

        if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(DeterminingDamage(collision));
            //
            
            // マージ解除
            if (TryGetComponent<MergedScrap>(out MergedScrap mergedScrap))
            {
                mergedScrap.Pearentbrake();
            }
        }



    }
    private void Update()
    {
        if(_isMergedLast != isMerged)
        {
            MergeInit();
            _isMergedLast = isMerged;
        }
        if (_lastState != scrapState)
        {
            switch (scrapState)
            {
                case ScrapState.Usually:
                    StateUsuallyInit();
                    break;

                case ScrapState.Tethered:
                    StateTetheredInit();
                    break;

                case ScrapState.Lifted:
                    SteteLiftedInit();
                    break;

                case ScrapState.InFlight:
                    StateInFlightInit();    
                    break;
            }
            _lastState = scrapState;
        }
    }
    private void StateUsuallyInit()
    {
        // 通常に戻る時の処理
        if (isMerged)
        {
            myRb.linearDamping = damp;
            gameObject.layer = 0;
        }
        else
        {
            // Scrapどうし接触するように。空中での摩擦ゼロも元に戻す。
            myRb.linearDamping = damp;
            gameObject.layer = 0;
        }
    }
    private void StateTetheredInit()
    {
        // ロープにつながれた瞬間の処理
        if (isMerged)
        {
            // ありえない
        }
        else
        {
            myRb.excludeLayers = LayerMask.GetMask("Ignore Collision");
        }
    }
    private void SteteLiftedInit()
    {
        // 持ち上げられた（rope内でのindexが0）瞬間の処理
        if (isMerged)
        {

        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Ignore Collision");
        }
    }
    private void StateInFlightInit()
    {
        // 投げられた瞬間の処理
        if (isMerged)
        {
            myRb.linearDamping = 0;
            myRb.excludeLayers = 0;
        }
        else
        {
            myRb.linearDamping = 0;
            myRb.excludeLayers = 0;
        }
    }

    private void MergeInit()
    {
        if (isMerged)
        {
            myRb.isKinematic = true;
            System.Array.ForEach(gameObject.GetComponents<Collider>(), c => c.enabled = false);
        }
        else
        {
            transform.parent = null;
            myRb.isKinematic = false;
        }

    }

    private float DeterminingDamage(Collision col)
    {
        float damage = 0f;
        float relativeSpeed = col.relativeVelocity.magnitude;

        if ( relativeSpeed < noDamageHitThreshold)
        {
            // ダメージなし
        }else
        {
            
            if (relativeSpeed >= criticalHitThreshold)
            {
                // 最大ダメージ
                damage += criticalDamage;
            }
            else if (relativeSpeed <= weakHitThreshold)
            {
                // 弱ダメージ
                damage += weakDamage;
            }
            else
            {
                // 通常ダメージ
                damage += baseDamage;
            }
        }
        return  damage;
    }
}




