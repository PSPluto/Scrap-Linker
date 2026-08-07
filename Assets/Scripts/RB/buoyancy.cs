using System;
using UnityEngine;

public class Buoyancy : MonoBehaviour
{ 
    public Rigidbody rb;
    public float upForce = 20f;

    
    private void Awake()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
    }
}
