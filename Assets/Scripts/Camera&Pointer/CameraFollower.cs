using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 3.5f, -3);
    [SerializeField] private float smoothTime = 0.3f;
    private Vector3 currentVelocity = Vector3.zero;

    // カメラをスムーズに追従させる
    public void FollowTo(Vector3 targetTransform)
    {
        Vector3 targetPosition = targetTransform + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        //transform.LookAt(targetTransform);
    }
}
