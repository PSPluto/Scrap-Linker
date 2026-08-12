using System;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Transform target;
    [SerializeField] public Vector3 offset = new Vector3(0, 3.5f, -3);
    public Vector3 currentOffset;
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private float rotationSmoothSpeed = 5f; // 回転の滑らかさ
    private Vector3 currentVelocity = Vector3.zero;

    private void Awake()
    {
        currentOffset = offset;
    }

    // カメラをスムーズに追従させる
    public void FollowTo(Vector3 targetTransform)
    {
        Vector3 targetPosition = targetTransform + currentOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

        // 目標方向への回転を滑らかに補間
        Vector3 lookDirection = targetTransform - transform.position;
        if (lookDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
        }
    }
}