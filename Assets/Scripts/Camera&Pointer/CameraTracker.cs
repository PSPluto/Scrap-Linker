using UnityEngine;

[RequireComponent(typeof(MouseWorldPointer))]
[RequireComponent(typeof(CameraFollower))]
public class CameraTracker : MonoBehaviour
{
    public Transform target;
    public TensileForceApplier tensileApplier;

    private MouseWorldPointer pointer;
    private CameraFollower follower;

    void Awake()
    {
        pointer = GetComponent<MouseWorldPointer>();
        follower = GetComponent<CameraFollower>();
    }

    void LateUpdate()
    {
        Vector3 castPos = pointer.Raycast().GetValueOrDefault(pointer.GetLastPosOrDefault());

        follower.FollowTo(Vector3.Lerp(target.position, castPos, 0.1f));
        tensileApplier.ApplyForceAt(castPos);
    }
}
