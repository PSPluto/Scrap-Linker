using UnityEngine;

public class WallObstacle : MonoBehaviour, IDamageable
{
    [SerializeField] private BoxCollider thisCollider;
    public float MaxDurability = 10;
    public float CurrentDurability = 10;


    public void TakeDamage(float damageAmount)
    {
        CurrentDurability -= damageAmount;

        if (CurrentDurability <= 0)
        {
            // 破壊処理
            thisCollider.enabled = false;
            foreach (Transform child in (transform.GetChild(0).transform))
            {
                Rigidbody childRb = (child.GetComponent<Rigidbody>());
                if (childRb != null)
                {
                    childRb.isKinematic = false;
                    childRb.AddForce(new Vector3(Random.Range(-3, 3), Random.Range(-3, 3), Random.Range(-3, 3)),
                        ForceMode.Impulse);
                    Destroy(gameObject, 3f);
                }
            }
        }
        else
        {
            CurrentDurability = MaxDurability;
        }
    }
}
