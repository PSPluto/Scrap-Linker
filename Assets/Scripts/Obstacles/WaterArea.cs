using System;
using System.Collections.Generic;
using UnityEngine;

public class WaterArea : MonoBehaviour
{
    [SerializeField] private float submergeAccel = 2f;
    [SerializeField] private float maxForceMultiplier = 2f;

    private Dictionary<Buoyancy, float> submergedTime = new Dictionary<Buoyancy, float>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Buoyancy>(out Buoyancy buoy))
        {
            submergedTime[buoy] = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Buoyancy>(out Buoyancy buoy))
        {
            submergedTime.Remove(buoy);
        }
    }

    private void FixedUpdate()
    {
        List<Buoyancy> keys = new List<Buoyancy>(submergedTime.Keys);

        foreach (var buoy in keys)
        {
            submergedTime[buoy] += Time.fixedDeltaTime;
            float t = submergedTime[buoy];

            // 時間経過で力が強くなる(最大値でクランプ)
            float multiplier = Mathf.Min(1f + t * submergeAccel, maxForceMultiplier);
            float force = buoy.upForce * multiplier;

            buoy.rb.AddForce(new Vector3(0, force, 0), ForceMode.Force);
        }
    }
}