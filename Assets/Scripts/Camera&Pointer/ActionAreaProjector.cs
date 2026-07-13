using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class ActionAreaProjector : MonoBehaviour
{
    [SerializeField] float multiplier = 0.2f;
    //描画側スクリプト
    [SerializeField]MouseWorldPointer worldPointer;
    [SerializeField]Transform cameraTransform;
    //[SerializeField]DecalProjector decalProjector;

    // Update is called once per frame
    void Update()
    {
        Vector3 pointer = worldPointer.GetLastPosOrDefault().point;
        Vector3 pointernomal = worldPointer.GetLastPosOrDefault().normal * multiplier;
        //transform.position = pointer + new Vector3(0, 5, 0);
        transform.position = pointer + pointernomal;
        transform.LookAt(pointer);
    }
}
