using System;
using UnityEngine;

public class Buoyancy : MonoBehaviour
{ 
    public Rigidbody rb;
    public float upForce = 20f;

    [Tooltip("水面で自然に水平姿勢へ戻ろうとするトルクの強さ。0で無効")]
    public float uprightTorque = 2f;

    
    private void Awake()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
    }
}