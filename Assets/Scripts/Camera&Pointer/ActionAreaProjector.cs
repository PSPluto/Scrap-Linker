using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class ActionAreaProjector : MonoBehaviour
{
    //描画側スクリプト
    [SerializeField]MouseWorldPointer worldPointer;
    [SerializeField]Transform cameraTransform;
    //[SerializeField]DecalProjector decalProjector;

    // Update is called once per frame
    void Update()
    {
        Vector3 pointer = worldPointer.GetLastPosOrDefault();
        transform.position = Vector3.Lerp(pointer, cameraTransform.position, 0.25f);
        transform.LookAt(pointer);

    }
}
