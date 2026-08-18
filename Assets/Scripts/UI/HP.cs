using System;
using UnityEngine;
using UnityEngine.UI;

public class HP : MonoBehaviour
{
    [SerializeField] private Image hpImage;
    [SerializeField] private float hpWeight = 1;

    private void Update()
    {
        var maxHp = PlayerController.Instance.maxHp;
        var currentHp = PlayerController.Instance.currentHp;
        hpWeight = ((currentHp/maxHp));
        hpImage.fillAmount = hpWeight;
    }
}
