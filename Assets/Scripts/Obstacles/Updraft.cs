using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Updraft : MonoBehaviour
{
    [SerializeField] private float upwardForce = 15f;
    [SerializeField] private ForceMode forceMode = ForceMode.Force;
    [SerializeField] private float maxUpwardSpeed = 20f;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;
        if (maxUpwardSpeed > 0f && rb.linearVelocity.y >= maxUpwardSpeed) return;

        rb.AddForce(Vector3.up * upwardForce, forceMode);
    }
}