using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(MouseWorldPointer))]
[RequireComponent(typeof(CameraFollower))]
public class CameraTracker : MonoBehaviour
{
    public Transform target;
    public TensileForceApplier tensileApplier;

    private MouseWorldPointer pointer;
    private CameraFollower follower;
    [SerializeField] private float width = 0.1f;
    private float currentWidth = 0.1f;


    void Awake()
    {
        pointer = GetComponent<MouseWorldPointer>();
        follower = GetComponent<CameraFollower>();
    }

    void LateUpdate()
    {
        Vector3 castPos = pointer.Raycast().GetValueOrDefault(pointer.GetLastPosOrDefault().point);

        follower.FollowTo(Vector3.Lerp(target.position, castPos, width));
        // tensileApplier.ApplyForceAt(castPos);
        if (Mouse.current.middleButton.isPressed)
        {
            currentWidth = Mathf.Lerp(currentWidth, 0f, 0.5f);
            follower.currentOffset = Vector3.Lerp(follower.currentOffset, new Vector3(0f,10f,-5f), 0.2f);
        }
        else
        {
            currentWidth = Mathf.Lerp(currentWidth, width, 0.5f);
            follower.currentOffset = Vector3.Lerp(follower.currentOffset, follower.offset, 0.2f);
        }
    }
}
