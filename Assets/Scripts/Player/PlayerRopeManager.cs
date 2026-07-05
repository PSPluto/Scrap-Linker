using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerRopeManager : MonoBehaviour
{
    [Header("ロープについてるオブジェクトのリスト")]
    public List<GameObject> towList = new List<GameObject>();
    [Header("ロープの長さ")]
    public float ropeLength = 5f;

    void FixedUpdate()
    {
        UpdateRope();
    }

    public void UpdateRope()
    {
        for (int i = 0; i < towList.Count; i++)
        {
            Vector3 direction = Vector3.zero;
            GameObject towObject = towList[i];
            float distance = 0f;
            Vector3 newPosition;
            if (i == 0)
            {
                direction = (towObject.transform.position - transform.position).normalized;
                distance = Vector3.Distance(towObject.transform.position, transform.position);
                // ロープの長さを超えた場合、オブジェクトを引き寄せる
                newPosition = transform.position + direction * ropeLength;
            }
            else
            {
                direction = (towObject.transform.position - towList[i - 1].transform.position).normalized;
                distance = Vector3.Distance(towObject.transform.position, towList[i - 1].transform.position);
                // ロープの長さを超えた場合、オブジェクトを引き寄せる
                newPosition = towList[i - 1].transform.position + direction * ropeLength;
            }
            if (distance > ropeLength)
            {

                towObject.GetComponent<Rigidbody>().MovePosition(newPosition);
            }
        }
    }
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Scrap"))
        {
            BaseScrap scrap = collision.gameObject.GetComponent<BaseScrap>();
            if (scrap.isTethered == false)
            {
                scrap.isTethered = true;
                towList.Add(collision.gameObject);
            }
        }
    }
}
