using UnityEngine;

public class BaseEnemy : MonoBehaviour , IDamageable
{
    private static readonly int AiState = Animator.StringToHash("AiState");
    [SerializeField]private float maxHp = 40;
    private float currentHp;
    [SerializeField]private float attackDamage = 20f;
    bool exitQueue = false;

    [SerializeField]private Animator animator;
    // Update is called once per frame
    public enum EnemyState
    {
        Idle,
        Attacking,
        Chase,
    }
    public EnemyState state;

    void Start()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHp -= damageAmount;
        Debug.Log($"-{damageAmount}({currentHp})");
        if (currentHp <= 0)
        {
            Debug.Log($"倒された");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            animator.SetInteger(AiState, 2);
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bool exitQueue = true;
        }
    }

    public void EndAttack()
    {
        if (!exitQueue)
        {
            return;
        }
        Debug.Log("攻撃終わり");
        animator.SetInteger(AiState, 0);
        exitQueue = false;

    }

    public void Attack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2f);
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == this.gameObject)
            {
                continue;
            }
            if (hitCollider.CompareTag("Player"))
            {
                hitCollider.GetComponent<IDamageable>().TakeDamage(attackDamage);
                
            }
            if(hitCollider.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                Vector3 knockbackDir = hitCollider.transform.position - transform.position;
                knockbackDir.y = 0.4f; 
                knockbackDir.Normalize();
                rb.AddForce(knockbackDir * 10f, ForceMode.VelocityChange);
            }
            
        }
    }
}
