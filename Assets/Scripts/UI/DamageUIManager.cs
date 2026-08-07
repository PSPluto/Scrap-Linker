using System;
using TMPro;
using UnityEngine;

public class DamageUIManager : MonoBehaviour
{
    public static DamageUIManager Instance { get; private set; } 
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Transform cameraTransform;

    private void Awake()
    {
        Instance = this;
    }

    public void NewDamageText(float damage, Vector3 pos)
    {
        var textObj = Instantiate(textPrefab, cameraTransform.position, Quaternion.identity);
        textObj.transform.GetChild(0).GetComponent<TextMeshPro>().text = damage.ToString();
        
        textObj.transform.LookAt(cameraTransform.position * -1);
        textObj.transform.position = Vector3.Lerp(pos, cameraTransform.position, 0.3f);
        
        Destroy(textObj, 0.35f);
    }
}
