using Unity.VisualScripting;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{

    [SerializeField] private float offset = 1f; 

    void OnCollisionEnter( Collision collision)
    {
        if (collision.relativeVelocity.magnitude < 2f)
        {
            Debug.Log($"何かが接触しましたが勢いが足りませんでした：{collision.relativeVelocity.magnitude}");
            return;
        }
        Vector3 newRespawnPos = (this.transform.forward * 1) + this.transform.position + new Vector3(0,1,0);
        PlayerController.Instance.respawnPos = newRespawnPos;
        Debug.Log($"セーブポイントが{newRespawnPos}");
    }
}
