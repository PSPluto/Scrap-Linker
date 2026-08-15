using System;
using System.Collections.Generic;
using UnityEngine;

public class Rail : MonoBehaviour
{
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lateralDamping = 10f; // レール方向以外を弱める強さ

    private HashSet<Rigidbody> rails = new HashSet<Rigidbody>();

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody; // GetComponentより安全・軽量

        if (rb != null)
        {
            rails.Add(rb);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        rails.RemoveWhere(rb => rb == null);

        Vector3 railDir = transform.right; // レールの進行方向

        foreach (Rigidbody onRailObj in rails)
        {
            // 進行方向への推進力
            onRailObj.AddForce(railDir * speed, ForceMode.Acceleration);

            // 現在の速度を「レール方向成分」と「それ以外の成分」に分解
            Vector3 velocity = onRailObj.linearVelocity;
            Vector3 alongRail = Vector3.Project(velocity, railDir);
            Vector3 lateral = velocity - alongRail; // レール方向以外の成分

            // それ以外の成分だけ減衰させる
            lateral = Vector3.Lerp(lateral, Vector3.zero, lateralDamping * Time.fixedDeltaTime);

            onRailObj.linearVelocity = alongRail + lateral;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            rails.Remove(rb);
        }
    }
}