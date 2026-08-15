using System;
using UnityEngine;

public class DamageArea : MonoBehaviour
{
[SerializeField] private float damage;
[SerializeField] private bool isScrapRespawn = true;

private void OnTriggerEnter(Collider other)
{
    PlayerController player = other.GetComponent<PlayerController>();
    if (player != null)
    {
        player.TakeDamage(damage);
    }
    else
    {
        BaseScrap baseScrap = other.GetComponent<BaseScrap>();
        if (baseScrap != null)
        {
            baseScrap.ResetPos();
        }
    }
}
}
