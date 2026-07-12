using UnityEngine;

[RequireComponent(typeof(MouseWorldPointer))]
[RequireComponent(typeof(CameraFollower))]
public class CameraTracker : MonoBehaviour
{
    public Transform target;
    public TensileForceApplier tensileApplier;

    private MouseWorldPointer pointer;
    private CameraFollower follower;
    [SerializeField] private float width = 0.1f;

    void Awake()
    {
        pointer = GetComponent<MouseWorldPointer>();
        follower = GetComponent<CameraFollower>();
    }

    void LateUpdate()
    {
        Vector3 castPos = pointer.Raycast().GetValueOrDefault(pointer.GetLastPosOrDefault().point);

        follower.FollowTo(Vector3.Lerp(target.position, castPos, width));
        tensileApplier.ApplyForceAt(castPos);
    }
}
