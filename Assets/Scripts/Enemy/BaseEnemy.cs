using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 敵AIのベースクラス。
/// 継承して独自の敵処理（ステート遷移や攻撃処理など）を実装・拡張できます。
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class BaseEnemy : MonoBehaviour, IDamageable
{
    // アニメーターのパラメータID（"AiState"のハッシュ値）
    private static readonly int AiState = Animator.StringToHash("AiState");

    /// <summary>
    /// 敵の状態を表す列挙型
    /// </summary>
    public enum EnemyState 
    { 
        Idle,   // 待機
        Chase,  // 追跡
        Attack, // 攻撃
        Die     // 死亡
    }

    [Header("ステート管理")]
    [Tooltip("現在のステート")]
    public EnemyState state = EnemyState.Idle;
    [Tooltip("1つ前のステート")]
    protected EnemyState lastState;
    
    [Header("HPと攻撃ダメージ")]
    [Tooltip("現在のHP")]
    protected float currentHp;
    [Tooltip("最大HP")]
    [SerializeField] protected float maxHp = 40f;
    [Tooltip("攻撃ダメージ")]
    [SerializeField] protected float attackDamage = 20f;
    
    [Header("距離設定")]
    [Tooltip("距離計算用のターゲット（Player）のTransform")]
    protected Transform player;
    [Tooltip("発見距離（この範囲内に入ると追跡開始）")][SerializeField] protected float detectionRange = 8f;
    [Tooltip("見失う距離（この範囲外に出ると待機へ遷移）")][SerializeField] protected float loseRange = 12f;
    [Tooltip("攻撃遷移距離（この範囲内に入ると攻撃開始）")][SerializeField] protected float attackRange = 1.6f;

    [Header("NavMeshとアニメーターの参照")]
    [Tooltip("移動を管理するNavMeshAgent")]
    [SerializeField] protected NavMeshAgent agent;
    [Tooltip("アニメーションを管理するAnimator")]
    [SerializeField] protected Animator animator;
    
    [Header("死亡遅延")] 
    [Tooltip("死亡ステート遷移からオブジェクトが破棄されるまでの遅延時間（秒）")]
    [SerializeField] protected float deathDelay;
    
    
    [Header("音")]
    [SerializeField]private AudioClip deathSound; 
    [SerializeField]private AudioClip damageSound;
    [SerializeField] private AudioClip attackSound;
    protected virtual void Start()
    {
        InitializeStatus();
        InitializeComponents();
        // 初期ステートを待機状態（Idle）に設定
        ChangeState(EnemyState.Idle);
    }

    /// <summary>
    /// ステート・ステータス（HPなど）の初期化処理。
    /// 独自変数の初期化が必要な場合は派生クラスでoverrideしてください。
    /// </summary>
    protected virtual void InitializeStatus()
    {
        currentHp = maxHp;
    }

    /// <summary>
    /// コンポーネントおよび参照の初期化処理。
    /// 追加のコンポーネント取得が必要な場合は派生クラスでoverrideしてください。
    /// </summary>
    protected virtual void InitializeComponents() 
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (PlayerController.Instance) player = PlayerController.Instance.transform;
    }

    protected virtual void Update()
    {
        // 死亡状態またはプレイヤー消失チェック（該当する場合は処理を中断）
        if (CheckDeathOrMissingPlayer()) return;

        // 現在のステートに応じた更新処理の実行
        UpdateCurrentState();

        // DamageUIManager.Instance.NewDamageText(111, this.transform.position);
    }

    /// <summary>
    /// 死亡判定またはプレイヤーの存在チェック。
    /// 死亡している、またはプレイヤーが存在しない場合は一定時間後に破棄します。
    /// </summary>
    protected virtual bool CheckDeathOrMissingPlayer()
    {
        if (state == EnemyState.Die || !player)
        {
            Destroy(gameObject, deathDelay);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 現在のステートに応じたロジックの分岐処理。
    /// </summary>
    protected virtual void UpdateCurrentState()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case EnemyState.Idle:
                UpdateIdleState(dist);
                break;
            case EnemyState.Chase:
                UpdateChaseState(dist);
                break;
            case EnemyState.Attack:
                UpdateAttackState();
                break;
        }
    }

    /// <summary>
    /// 待機ステート（Idle）時の更新処理。
    /// プレイヤーが発見距離内に入ったら追跡ステートへ遷移します。
    /// </summary>
    protected virtual void UpdateIdleState(float dist)
    {
        if (dist <= detectionRange) ChangeState(EnemyState.Chase);
    }

    /// <summary>
    /// 追跡ステート（Chase）時の更新処理。
    /// 距離に応じて「見失う（Idle）」「攻撃する（Attack）」「移動継続」を判定します。
    /// </summary>
    protected virtual void UpdateChaseState(float dist)
    {
        if (dist > loseRange) ChangeState(EnemyState.Idle);
        else if (dist <= attackRange) ChangeState(EnemyState.Attack);
        else agent.SetDestination(player.position);
    }

    /// <summary>
    /// 攻撃ステート（Attack）時の更新処理。
    /// ターゲットの方向へ回転します。
    /// </summary>
    protected virtual void UpdateAttackState()
    {
        FaceTarget(player.position);
    }

    /// <summary>
    /// 攻撃実行メソッド（アニメーションイベントなどから呼び出すことを想定）。
    /// 当たり判定を取得してダメージ・ノックバック処理を行います。
    /// </summary>
    public virtual void Attack()
    {
        Collider[] hitColliders = GetHitColliders();
        ProcessAttackHits(hitColliders);
        AudioManager.Instance.PlaySound(attackSound, transform.position);
    }

    /// <summary>
    /// 攻撃の当たり判定範囲内にいるColliderを取得します。
    /// </summary>
    protected virtual Collider[] GetHitColliders()
    {
        return Physics.OverlapSphere(transform.position, 1.4f);
    }

    /// <summary>
    /// 検出された各Colliderに対してダメージおよびノックバック処理を適用します。
    /// </summary>
    protected virtual void ProcessAttackHits(Collider[] hitColliders)
    {
        foreach (Collider hitCollider in hitColliders)
        {
            // 自分自身にはヒットさせない
            if (hitCollider.gameObject == gameObject) continue;

            TryDealDamage(hitCollider);
            TryApplyKnockback(hitCollider);
        }
    }

    /// <summary>
    /// プレイヤーに対してダメージを与える処理。
    /// </summary>
    protected virtual void TryDealDamage(Collider hitCollider)
    {
        if (hitCollider.CompareTag("Player") && hitCollider.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(attackDamage);
        }
    }

    /// <summary>
    /// 対象にRigidbodyがある場合、ノックバック力を加えます。
    /// </summary>
    protected virtual void TryApplyKnockback(Collider hitCollider)
    {
        if (hitCollider.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            Vector3 knockbackDir = CalculateKnockbackDirection(hitCollider.transform.position);
            rb.AddForce(knockbackDir * 40f, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// ノックバックの方向ベクトルを計算します（少し上向きの補正を含む）。
    /// </summary>
    protected virtual Vector3 CalculateKnockbackDirection(Vector3 targetPosition)
    {
        Vector3 knockbackDir = (targetPosition - transform.position).normalized;
        knockbackDir.y = 0.4f;
        return knockbackDir;
    }

    /// <summary>
    /// 攻撃アニメーション終了時に呼び出すメソッド。
    /// 距離に応じて次のステート（AttackまたはChase）へ自動遷移します。
    /// </summary>
    public virtual void EndAttack()
    {
        if (state == EnemyState.Die || !player) return;

        float dist = Vector3.Distance(transform.position, player.position);
        ChangeState(dist <= attackRange ? EnemyState.Attack : EnemyState.Chase);
    }

    /// <summary>
    /// ダメージを受け取るインターフェース実装処理。
    /// HPを減らし、0以下になった場合は死亡ステート（Die）へ遷移します。
    /// </summary>
    /// <param name="damageAmount">受けるダメージ量</param>
    public virtual void TakeDamage(float damageAmount)
    {
        if (state == EnemyState.Die) return;

        currentHp -= damageAmount;
        if (currentHp <= 0)
        {
            ChangeState(EnemyState.Die);
            AudioManager.Instance.PlaySound(deathSound, transform.position);
        }
        else
        {
            AudioManager.Instance.PlaySound(damageSound, transform.position);
        }
}

    /// <summary>
    /// ステートを変更し、アニメーターおよびNavMeshAgentの状態を同期します。
    /// </summary>
    /// <param name="newState">遷移先のステート</param>
    protected virtual void ChangeState(EnemyState newState)
    {
        lastState = state;
        state = newState;
        animator.SetInteger(AiState, (int)newState);
        // 追跡（Chase）ステート以外の時はNavMeshAgentの移動を停止する
        if (agent) agent.isStopped = (newState != EnemyState.Chase);
    }

    /// <summary>
    /// 指定したターゲット位置の方向をなめらかに向く（Y軸回転のみ）。
    /// </summary>
    /// <param name="targetPosition">向きたい目標座標</param>
    protected virtual void FaceTarget(Vector3 targetPosition)
    {
        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f; // Y軸の傾きを無視して水平回転のみにする
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
    }
    
    /// <summary>
    /// Sceneビューで各種検知範囲（発見・見失う・攻撃）を球体ギズモとして視覚化します。
    /// </summary>
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange); // 黄色：発見距離

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, loseRange);      // 青色：見失う距離

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);    // 赤色：攻撃遷移距離
    }
}