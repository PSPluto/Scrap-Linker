using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaterArea : MonoBehaviour
{
    private HashSet<Buoyancy> buoyancies = new HashSet<Buoyancy>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Buoyancy>(out Buoyancy buoy))
        {
            buoyancies.Add(buoy);
            Debug.Log(buoyancies.Count);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Buoyancy>(out Buoyancy buoy))
        {
            buoyancies.Remove(buoy);
        }
    }

    private void FixedUpdate()
    {
        foreach (var buoyancyObj in buoyancies)
        {
            buoyancyObj.rb.AddForce(new Vector3(0, buoyancyObj.upForce, 0), ForceMode.Force);
        }
    }
}