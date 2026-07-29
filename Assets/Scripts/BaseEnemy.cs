using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BaseEnemy : MonoBehaviour, IDamageable
{
    private static readonly int AiState = Animator.StringToHash("AiState");

    public enum EnemyState { Idle, Chase, Attack, Die }
    public EnemyState state = EnemyState.Idle;

    [SerializeField] private float maxHp = 40f;
    [SerializeField] private float attackDamage = 20f;

    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float loseRange = 12f;
    [SerializeField] private float attackRange = 1.6f;

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    private float currentHp;
    private Transform player;

    private void Start()
    {
        currentHp = maxHp;
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (PlayerController.Instance) player = PlayerController.Instance.transform;
        ChangeState(EnemyState.Idle);
    }

    private void Update()
    {
        if (state == EnemyState.Die || !player)
        {
            Destroy(gameObject, 1f);
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case EnemyState.Idle:
                if (dist <= detectionRange) ChangeState(EnemyState.Chase);
                break;

            case EnemyState.Chase:
                if (dist > loseRange) ChangeState(EnemyState.Idle);
                else if (dist <= attackRange) ChangeState(EnemyState.Attack);
                else agent.SetDestination(player.position);
                break;

            case EnemyState.Attack:
                FaceTarget(player.position);
                break;
        }
    }

    public void Attack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1.4f);
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == gameObject) continue;

            if (hitCollider.CompareTag("Player") && hitCollider.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(attackDamage);
            }

            if (hitCollider.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                Vector3 knockbackDir = (hitCollider.transform.position - transform.position).normalized;
                knockbackDir.y = 0.4f;
                rb.AddForce(knockbackDir * 10f, ForceMode.VelocityChange);
            }
        }
    }

    public void EndAttack()
    {
        if (state == EnemyState.Die || !player) return;

        float dist = Vector3.Distance(transform.position, player.position);
        ChangeState(dist <= attackRange ? EnemyState.Attack : EnemyState.Chase);
    }

    public void TakeDamage(float damageAmount)
    {
        if (state == EnemyState.Die) return;

        currentHp -= damageAmount;
        if (currentHp <= 0) ChangeState(EnemyState.Die);
    }

    private void ChangeState(EnemyState newState)
    {
        state = newState;
        animator.SetInteger(AiState, (int)newState);
        if (agent) agent.isStopped = (newState != EnemyState.Chase);
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
    }
}