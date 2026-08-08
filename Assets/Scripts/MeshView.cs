using UnityEngine;

public class MeshView : MonoBehaviour
{
    private void OnDrawGizmosSelected()
    {
        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc != null && mc.sharedMesh != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawWireMesh(mc.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
        }
    }
}
