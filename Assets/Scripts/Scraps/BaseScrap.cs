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
        [Tooltip("弱ヒットのパーティクル")] public GameObject weakHitParticle;

    [Header("・基本ヒット")]   
        [Tooltip("基本 ダメージ")] public float baseDamage;
        [Tooltip("基本ヒットの音")] public AudioClip baseHitSound;
        [Tooltip("基本ヒットのパーティクル")] public GameObject baseHitParticle;


    [FormerlySerializedAs("CriticalDamage")]
    [Header("・強ヒット")]
        [Tooltip("クリティカル ダメージ")] public float criticalDamage;
        [HideInInspector][Tooltip("強ヒットの閾値 (以上)")] public float criticalHitThreshold = 8;
        [Tooltip("強ヒットの音")] public AudioClip criticalHitSound;  
        [Tooltip("強ヒットのパーティクル")] public  GameObject criticalHitParticle;

    [Header("・ダメージ0")]
        [Tooltip("ダメージ0の音")] public AudioClip noDamageSound;
    [HideInInspector][Tooltip("Zeroヒットの閾値 (以下)")] public float noDamageHitThreshold = 3;
        [Tooltip("ダメージなしのパーティクル")] public GameObject noDamageHitParticle;


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
        int resultDamageLevel = DeterminingDamageLevel(collision);
        GameObject hitParticlePrefab = noDamageHitParticle;
        switch (resultDamageLevel)
        {
            case 0:
                // 前でやってるので問題ない
                AudioManager.Instance.PlayAudioOneShot(noDamageSound, transform.position);
                break;
            
            case 1:
                hitParticlePrefab = weakHitParticle;
                AudioManager.Instance.PlayAudioOneShot(weakHitSound, transform.position);
                break;
            
            case 2:
                hitParticlePrefab = baseHitParticle;
                AudioManager.Instance.PlayAudioOneShot(baseHitSound, transform.position);
                break;
            
            case 3:
                hitParticlePrefab = criticalHitParticle;
                AudioManager.Instance.PlayAudioOneShot(criticalHitSound, transform.position);
                break;
        }
        GameObject hitParticleObj = Instantiate(hitParticlePrefab, collision.GetContact(0).point, Quaternion.FromToRotation(Vector3.forward ,collision.GetContact(0).normal*-1));

        // ダメージを受けるモノに当たった場合
        if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            // ダメージ
            damageable.TakeDamage(ReturnLevelDamage(resultDamageLevel));
            
            //HITパーティクル
            
            
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

    private int DeterminingDamageLevel(Collision col)
    {
        int damageLevel = 0;
        float relativeSpeed = col.relativeVelocity.magnitude;

        if ( relativeSpeed < noDamageHitThreshold)
        {
            // ダメージなし
        }else
        {
            // ダメージアリ
            if (relativeSpeed >= criticalHitThreshold)
            {
                // 最大ダメージ
                damageLevel += 3;
            }
            else if (relativeSpeed <= weakHitThreshold)
            {
                // 弱ダメージ
                damageLevel += 1;
            }
            else
            {
                // 通常ダメージ
                damageLevel += 2;
            }
        }
        return  damageLevel;
    }

    public float ReturnLevelDamage(int damageLevel)
    {
        float resultDamage = 0;
        switch (damageLevel)
        {
            case 0:
                break;
            case 1:
                resultDamage = weakDamage;
                break;
            case 2:
                resultDamage = baseDamage;
                break;
            case 3:
                resultDamage = criticalDamage;
                break;
        }
        return resultDamage;
    }
}




