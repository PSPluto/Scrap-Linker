using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static RopeScrapElement;

public class PlayerRopeManager : MonoBehaviour
{
    public MouseWorldPointer mouseWorldPointer;
    [Header("���[�v�ɂ��Ă�I�u�W�F�N�g�̃��X�g")]
    public List<RopeElement> towList = new List<RopeElement>();
    [Header("���[�v�̒���")]
    public float ropeLength = 5f;

    bool isLeftClicked;
    bool isRightClicked;

    public float requiredSpeed = 4f;

    void FixedUpdate()
    {
        UpdateRope();
        if (isLeftClicked == true)
        {
            if (towList.Count > 0)
            {
                ThrowingScrap(0);
            }
            isLeftClicked = false;
        }

    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isLeftClicked = true;
        }
        isRightClicked = Mouse.current.rightButton.wasPressedThisFrame;
    }

    public void UpdateRope()
    {
        for (int i = 0; i < towList.Count; i++)
        {
            Vector3 direction = Vector3.zero;
            GameObject towObject = towList[i].gameObj;
            float distance = 0f;
            Vector3 newPosition;
            if (i == 0)
            {
                newPosition = this.transform.position + new Vector3(0,1f,0);
                towList[i].rb.MovePosition(newPosition);
                towList[i].rb.linearVelocity = new Vector3(0, 0, 0);
                continue;
            }
            else
            {
                if (i == 1)
                {
                    direction = (towObject.transform.position - transform.position).normalized;
                    distance = Vector3.Distance(towObject.transform.position, transform.position);
                    newPosition = transform.position + direction * ropeLength;
                }
                else
                {
                    direction = (towObject.transform.position - towList[i - 1].gameObj.transform.position).normalized;
                    distance = Vector3.Distance(towObject.transform.position, towList[i - 1].gameObj.transform.position);
                    newPosition = towList[i - 1].gameObj.transform.position + direction * ropeLength;
                }
            }
            if (distance > ropeLength)
            {

                towList[i].rb.MovePosition(newPosition);
            }
        }
    }
    public void OnCollisionEnter(Collision collision)
    {
        // ロープへの登録
        if (collision.gameObject.CompareTag("Scrap"))
        {
            BaseScrap scrap = collision.gameObject.GetComponent<BaseScrap>();
            collision.gameObject.layer = LayerMask.NameToLayer("Ignore Collision");
            if (scrap.isTethered == false)
            {
                scrap.isTethered = true;
                towList.Add(new RopeElement{
                    gameObj = collision.gameObject,
                    rb = collision.gameObject.GetComponent<Rigidbody>()
                });
            }
        }
    }
    public void ThrowingScrap(int removeIndex)
    {
        GameObject removeObj = towList[removeIndex].gameObj;
        removeObj.layer = 0;
        removeObj.GetComponent<BaseScrap>().isTethered = false;

        Rigidbody removeObjRb = towList[removeIndex].rb;
        removeObjRb.linearDamping = 0f;


       removeObjRb.AddForce(
            ThrowVectorGetter.CalculateLaunchVectorWithApexHeight(
                removeObj.transform.position,
                mouseWorldPointer.GetLastPosOrDefault(),
                2.0f,
                out requiredSpeed
            ),
            ForceMode.VelocityChange
        );

        towList.RemoveAt(removeIndex);

    }

}
