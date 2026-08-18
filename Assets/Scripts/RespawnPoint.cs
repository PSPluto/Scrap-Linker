using System;
using Unity.VisualScripting;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{

    [SerializeField] private float offset = 1f;
    [SerializeField] private bool isStartPoint = false;
    [SerializeField] private GameObject hitParticlePrefab;
    [SerializeField] private AudioClip setRespawnPointSound;
    private void Start()
    {
        if (isStartPoint)
        {
            SetRespawnPoint();
        }
    }

    void OnCollisionEnter( Collision collision)
    {
        if (PlayerController.Instance.respawnPos == (this.transform.forward * 1) + this.transform.position + new Vector3(0,1,0))
        {
            return;
        }
        SetRespawnPoint();
    }

    void SetRespawnPoint()
    {
        Vector3 newRespawnPos = (this.transform.forward * 1) + this.transform.position + new Vector3(0,1,0);
        PlayerController.Instance.respawnPos = newRespawnPos;
        Debug.Log($"セーブポイントが{newRespawnPos}");
        Instantiate(hitParticlePrefab, this.transform.position + new Vector3(0,1.5f,0), Quaternion.identity);
        AudioManager.Instance.PlaySound(setRespawnPointSound,this.transform.position);

    }
}
