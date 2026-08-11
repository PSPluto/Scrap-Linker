using System;
using System.Collections.Generic;
using UnityEngine;

public class Rail : MonoBehaviour
{
    [SerializeField]private float speed = 25f;
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
        foreach (Rigidbody onRailObj in rails)
        {
            onRailObj.AddForce(transform.right * speed, ForceMode.Acceleration);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        rails.Remove(other.GetComponent<Rigidbody>());
    }
}
