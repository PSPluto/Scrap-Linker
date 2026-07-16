using UnityEngine;
using UnityEngine.InputSystem;


public class TensileForceApplier : MonoBehaviour
{
    public Transform player;
    public float tensileForce;
    public float tensileAreaSize;

    private Vector3 debugPos;

    // OverlapSphereで重なっているオブジェクトをプレイヤーに引き寄せる。
    public void ApplyForceAt(Vector3 pos)
    {
        if (Mouse.current != null)
        {
           if (Mouse.current.middleButton.isPressed)
            {
                debugPos = pos;
                Collider[] hitColliders = new Collider[20];
                int hitCount = Physics.OverlapSphereNonAlloc(pos, tensileAreaSize, hitColliders);

                for (int i = 0; i < hitCount; i++)
                {
                    Rigidbody colliderRb = hitColliders[i].GetComponent<Rigidbody>();
                    if (colliderRb == null) continue;

                    BaseScrap scrap = colliderRb.gameObject.GetComponent<BaseScrap>();
                    if (scrap != null && scrap.scrapState == BaseScrap.ScrapState.usually)
                    {
                        Vector3 dir = (colliderRb.position - player.position).normalized;
                        colliderRb.AddForce(dir * (-1 * tensileForce), ForceMode.Acceleration);
                    }
                }
            }
        }
    }

    // デバッグ用にエリアwo
    // 描画
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(debugPos, tensileAreaSize);
    }
}
