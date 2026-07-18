using UnityEngine;

public class WallObstacle : MonoBehaviour
{
    [SerializeField] private BoxCollider thisCollider;
    [SerializeField] private float threshold;
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"{collision}：{collision.impulse.magnitude / Time.fixedDeltaTime}");
        //if ((collision.impulse.magnitude / Time.fixedDeltaTime) > 150)
        //{
        //    MergedScrap mergedScrap = collision.gameObject.GetComponent<MergedScrap>();
        //    if (mergedScrap != null)
        //    {
        //        mergedScrap.Pearentbrake();
        //    }
        //}
        if ((collision.impulse.magnitude / Time.fixedDeltaTime) <= threshold)
        {
            return;
        }
        thisCollider.enabled = false;
        foreach (Transform child in (transform.GetChild(0).transform))
        {
            Rigidbody childRb = (child.GetComponent<Rigidbody>());
            if (childRb != null)
            {
                childRb.isKinematic = false;
                childRb.AddForce(new Vector3(Random.Range(-3, 3), Random.Range(-3, 3), Random.Range(-3, 3)), ForceMode.Impulse);
                Destroy(gameObject, 3f);
            }
        }
    }
}
