using UnityEngine;

public class BaseScrap : MonoBehaviour
{

    public enum ScrapState
    {
        // ロープに従っている状態
        Tethered,
        // 投げられている状態
        InFlight,
        // 通常状態
        usually
    }

    [Header ("=== Rigidbody ===")]
        [SerializeField] private Rigidbody myRb;
        [Tooltip("摩擦")]public float damp = 2;

    [Header ("=== Scrapの設定 ===")]

    [Header("・弱ヒット")]
        [Tooltip("弱 ダメージ")] public float weakDamage;
        [Tooltip("弱ヒットの閾値 (以下)")] public float weakHitThreshold;
        [Tooltip("弱ヒットの音")] public AudioClip weakHitSound;

    [Header("・基本ヒット")]   
        [Tooltip("基本 ダメージ")] public float baseDamage;
        [Tooltip("基本ヒットの音")] public AudioClip baseHitSound;


    [Header("・強ヒット")]
        [Tooltip("クリティカル ダメージ")] public float CriticalDamage;
        [Tooltip("強ヒットの閾値 (以上)")] public float criticalHitThreshold;
        [Tooltip("強ヒットの音")] public AudioClip criticalHitSound;  

    [Header("・ダメージ0")]
        [Tooltip("ダメージ0の閾値 (以下)")] public float noDamageThreshold;
        [Tooltip("ダメージ0の音")] public AudioClip noDamageSound;

    [Header("=== State ===")]
    [Tooltip("Scrapの状態")] public ScrapState scrapState = ScrapState.usually;
    private ScrapState lastState = ScrapState.usually;



    private void OnCollisionEnter(Collision collision)
    {
        if (scrapState == ScrapState.InFlight)
        {
            scrapState = ScrapState.usually;
        }
    }
    private void Update()
    {
        if (lastState != scrapState)
        {
            switch (scrapState)
            {
                case ScrapState.usually:
                    StateUsuallyInit();
                    break;

                case ScrapState.Tethered:
                    StateTetheredInit();
                    break;
                
                case ScrapState.InFlight:
                    StateInFlightInit();    
                    break;
            }
            lastState = scrapState;
        }
    }
    private void StateUsuallyInit()
    {
        // 通常に戻る時の処理
        myRb.linearDamping = damp;
        myRb.excludeLayers = 0;
    }
    private void StateTetheredInit()
    {
        // ロープにつながれた瞬間の処理
    }
    private void StateInFlightInit()
    {
        // 投げられた瞬間の処理
        myRb.linearDamping = 0;
        gameObject.layer = 0;
    }
}


