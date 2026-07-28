using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BaseEnemy : MonoBehaviour , IDamageable
{
    private static readonly int AiState = Animator.StringToHash("AiState");
    [SerializeField]private float maxHp = 40;
    private float currentHp;
    [SerializeField]private float attackDamage = 20f;
    bool exitQueue = false;
    [SerializeField]private Rigidbody myRb; 

    [SerializeField]private Animator animator;
    // Update is called once per frame
    public enum EnemyState
    {
        Idle,
        Attacking,
        Chase,
        Die,
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
        if (other.CompareTag("Player") && animator.GetInteger(AiState) == 0)
        {
            animator.SetInteger(AiState, 2);

            Vector3 direction = (PlayerController.Instance.transform.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                myRb.MoveRotation(targetRotation);
            }
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            exitQueue = true;
        }
    }

    public void EndAttack()
    {
        if (exitQueue == false)
        {
            animator.SetInteger(AiState, 1);
            StartCoroutine(Rush());
            exitQueue = false;
            return;
        }
        Debug.Log("攻撃終わり");
        animator.SetInteger(AiState, 0);
        exitQueue = false;

    }

    public void Attack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1.4f);
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

    IEnumerator Rush()
    {
        Debug.Log("コルーチンはじめ");
        float speed = 0f;
        int loopCount = 0;
        while (true)
        {
            loopCount++;
            myRb.linearVelocity = transform.forward * (-1 * (Mathf.Lerp(speed, 5, 0.2f)));
            if (loopCount == 40)
            {
                animator.SetInteger(AiState, 0);
                Debug.Log("コルーチン終わり");
                yield break;
            }
            yield return new WaitForSeconds(0.05f);   
        }
    }
}
