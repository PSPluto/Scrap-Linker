using UnityEngine;
using UnityEngine.InputSystem;

public class MouseWorldPointer : MonoBehaviour
{
    private RaycastHit lastCastHit;


    // マウス座標に向かってレイを飛ばし、ヒットした座標を返す
    public Vector3? Raycast()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            lastCastHit = hit;
            return hit.point;
        }
        return null;
    }

    // 1行で関数を宣言できる。returnもかける（ここではクラスで宣言されたメソッド外の変数を返してる。）。
    public RaycastHit GetLastPosOrDefault() => lastCastHit;
}
