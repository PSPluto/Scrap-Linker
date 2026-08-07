using System;
using UnityEngine;

public class Buoyancy : MonoBehaviour
{ 
    public Rigidbody rb;
    
    private void Awake()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
    }
}
