using System.Collections.Generic;
using UnityEngine;

public class MergedScrap : MonoBehaviour
{
    public void Pearentbrake()
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }
        foreach (Transform child in children)
        {
            
            child.parent = null;
            Rigidbody childRb = child.GetComponent<Rigidbody>();
            childRb.isKinematic = false;
            childRb.AddForce(new Vector3(Random.Range(-5, 5), Random.Range(3, 10), Random.Range(-5, 5)), ForceMode.VelocityChange);
            System.Array.ForEach(child.GetComponents<Collider>(), c => c.enabled = true);
            BaseScrap childScrap = child.GetComponent<BaseScrap>();
            childScrap.scrapState = BaseScrap.ScrapState.InFlight;
            childScrap.isMerged = false;
        }

        Destroy(gameObject);

    }
    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.relativeVelocity.magnitude) > 5)
        {
            Pearentbrake();
        }
    }

}
