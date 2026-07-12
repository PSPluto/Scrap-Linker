using UnityEngine;

public class BaseScrap : MonoBehaviour
{
    public bool isTethered = false;
    public float damp = 2;
    [SerializeField] private Rigidbody myRb;

    private void OnCollisionEnter(Collision collision)
    {
        myRb.linearDamping = damp;
        myRb.excludeLayers = 0;

    }
}
