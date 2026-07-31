using System;
using System.Threading.Tasks;
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

        if (collision.gameObject.TryGetComponent<PlayerController>(out PlayerController playerController))
        {
            playerController.debuffTime = Mathf.Lerp(playerController.debuffTime, 3f, 0.25f);
        }
        Destroy(gameObject);
    }

    private async void Start()
    {
        try
        {
            await Task.Delay(2000);
            GetComponent<Rigidbody>().useGravity = true;
        }
        catch (Exception)
        {
            return;
        }
    }
}
