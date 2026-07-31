using System;
using UnityEngine;

public class NitroBolt : MonoBehaviour
{
    public float damage = 3;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
