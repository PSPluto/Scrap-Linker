using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerRopeManager : MonoBehaviour
{
    public MouseWorldPointer mouseWorldPointer;
    [Header("���[�v�ɂ��Ă�I�u�W�F�N�g�̃��X�g")]
    public List<GameObject> towList = new List<GameObject>();
    [Header("���[�v�̒���")]
    public float ropeLength = 5f;

    bool isLeftClicked;
    bool isRightClicked;

    void FixedUpdate()
    {
        UpdateRope();

    }

    private void Update()
    {
        isLeftClicked = Mouse.current.leftButton.wasPressedThisFrame;
        isRightClicked = Mouse.current.rightButton.wasPressedThisFrame;

        if (isLeftClicked == true)
        {
            if (towList.Count > 0)
            {
                ThrowingScrap(towList.Count - 1);
            }
        }
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
                newPosition = transform.position + direction * ropeLength;
                //newPosition = this.transform.position + new Vector3(0, 1, 0);
            }
            else
            {
                direction = (towObject.transform.position - towList[i - 1].transform.position).normalized;
                distance = Vector3.Distance(towObject.transform.position, towList[i - 1].transform.position);
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
            collision.gameObject.layer = LayerMask.NameToLayer("Ignore Collision");
            if (scrap.isTethered == false)
            {
                scrap.isTethered = true;
                towList.Add(collision.gameObject);
            }
        }
    }
    public void ThrowingScrap(int removeIndex)
    {
        GameObject removeObj = towList[removeIndex];
        removeObj.layer = 0;
        removeObj.GetComponent<BaseScrap>().isTethered = false;
        towList.RemoveAt(removeIndex);
        removeObj.GetComponent<Rigidbody>().AddForce(ThrowVectorGetter.CalculateLaunchVectorWithFixedSpeed(removeObj.transform.position, mouseWorldPointer.GetLastPosOrDefault(), 20.0f), ForceMode.Impulse);
    }

}
