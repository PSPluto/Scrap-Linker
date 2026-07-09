using UnityEngine;
using static RopeScrapElement;

public class RopeRenderer : MonoBehaviour
{
    [SerializeField] Transform playerTransform;
    [SerializeField] PlayerRopeManager ropeManager;
    [SerializeField] LineRenderer lineRenderer;

    void Update()
    {
        RopeElement[] ropeElements = ropeManager.towList.ToArray();
        if (ropeElements.Length <= 1)
        {
            lineRenderer.positionCount = 0;
            return;
        }
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        lineRenderer.positionCount = ropeElements.Length;


        lineRenderer.SetPosition(0, playerTransform.position);
        for (int i = 1; i < ropeElements.Length; i++)
        {
            lineRenderer.SetPosition(i, ropeElements[i].gameObj.transform.position);
        }
    }
}
