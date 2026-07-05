using UnityEngine;
using UnityEngine.InputSystem;

public class MouseWorldPointer : MonoBehaviour
{
    private Vector3 lastCastPos = Vector3.zero;

    // マウス座標に向かってレイを飛ばし、ヒットした座標を返す
    public Vector3? Raycast()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            lastCastPos = hit.point;
            return hit.point;
        }
        return null;
    }

    public Vector3 GetLastPosOrDefault() => lastCastPos;
}
