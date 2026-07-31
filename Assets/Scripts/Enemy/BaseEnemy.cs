using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BaseEnemy : MonoBehaviour, IDamageable
{
    private static readonly int AiState = Animator.StringToHash("AiState");

    public enum EnemyState { Idle, Chase, Attack, Die }
    public EnemyState state = EnemyState.Idle;

    [SerializeField] protected float maxHp = 40f;
    [SerializeField] protected float attackDamage = 20f;

    [SerializeField] protected float detectionRange = 8f;
    [SerializeField] protected float loseRange = 12f;
    [SerializeField] protected float attackRange = 1.6f;

    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Animator animator;
    
    protected EnemyState lastState;
    
    [Header("死亡遅延")] [SerializeField] protected float deathDelay;
    
    protected float currentHp;
    protected Transform player;

    protected virtual void Start()
    {
        InitializeStatus();
        InitializeComponents();
        ChangeState(EnemyState.Idle);
    }

    protected virtual void InitializeStatus()
    {
        currentHp = maxHp;
    }

    protected virtual void InitializeComponents()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (PlayerController.Instance) player = PlayerController.Instance.transform;
    }

    protected virtual void Update()
    {
        if (CheckDeathOrMissingPlayer()) return;

        UpdateCurrentState();
    }

    protected virtual bool CheckDeathOrMissingPlayer()
    {
        if (state == EnemyState.Die || !player)
        {
            Destroy(gameObject, deathDelay);
            return true;
        }
        return false;
    }

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

    protected virtual void UpdateIdleState(float dist)
    {
        if (dist <= detectionRange) ChangeState(EnemyState.Chase);
    }

    protected virtual void UpdateChaseState(float dist)
    {
        if (dist > loseRange) ChangeState(EnemyState.Idle);
        else if (dist <= attackRange) ChangeState(EnemyState.Attack);
        else agent.SetDestination(player.position);
    }

    protected virtual void UpdateAttackState()
    {
        FaceTarget(player.position);
    }

    public virtual void Attack()
    {
        Collider[] hitColliders = GetHitColliders();
        ProcessAttackHits(hitColliders);
    }

    protected virtual Collider[] GetHitColliders()
    {
        return Physics.OverlapSphere(transform.position, 1.4f);
    }

    protected virtual void ProcessAttackHits(Collider[] hitColliders)
    {
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == gameObject) continue;

            TryDealDamage(hitCollider);
            TryApplyKnockback(hitCollider);
        }
    }

    protected virtual void TryDealDamage(Collider hitCollider)
    {
        if (hitCollider.CompareTag("Player") && hitCollider.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(attackDamage);
        }
    }

    protected virtual void TryApplyKnockback(Collider hitCollider)
    {
        if (hitCollider.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            Vector3 knockbackDir = CalculateKnockbackDirection(hitCollider.transform.position);
            rb.AddForce(knockbackDir * 20f, ForceMode.Impulse);
        }
    }

    protected virtual Vector3 CalculateKnockbackDirection(Vector3 targetPosition)
    {
        Vector3 knockbackDir = (targetPosition - transform.position).normalized;
        knockbackDir.y = 0.4f;
        return knockbackDir;
    }

    public virtual void EndAttack()
    {
        if (state == EnemyState.Die || !player) return;

        float dist = Vector3.Distance(transform.position, player.position);
        ChangeState(dist <= attackRange ? EnemyState.Attack : EnemyState.Chase);
    }

    public virtual void TakeDamage(float damageAmount)
    {
        if (state == EnemyState.Die) return;

        currentHp -= damageAmount;
        if (currentHp <= 0) ChangeState(EnemyState.Die);
    }

    protected virtual void ChangeState(EnemyState newState)
    {
        lastState = state;
        state = newState;
        animator.SetInteger(AiState, (int)newState);
        if (agent) agent.isStopped = (newState != EnemyState.Chase);
    }

    protected virtual void FaceTarget(Vector3 targetPosition)
    {
        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, loseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}