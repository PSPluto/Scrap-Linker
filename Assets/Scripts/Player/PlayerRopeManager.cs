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

    [SerializeField]private GameObject mergedScrapPrefab;

    void FixedUpdate()
    {
        if (isLeftClicked == true)
        {
            ThrowingScrap(0);
            isLeftClicked = false;
        }
        if (isRightClicked == true)
        {
            MargeThorowScrap();
            isRightClicked = false;
        }
        UpdateRope();

    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isLeftClicked = true;
        }
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            isRightClicked = true;
        }
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
                towList[i].gameObj.layer = LayerMask.NameToLayer("Ignore Collision");
                towList[i].rb.excludeLayers = LayerMask.GetMask("Ignore Collision");
                newPosition = this.transform.position + new Vector3(0,0.75f,0);
                towList[i].rb.MovePosition(newPosition);
                towList[i].rb.linearVelocity = new Vector3(0, 0, 0);
                towList[i].rb.MoveRotation(transform.rotation);
                continue;
            }
            else
            {
                towList[i].rb.excludeLayers = 0;
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
                towList[i].rb.MovePosition(Vector3.Lerp(towList[i].gameObj.transform.position, newPosition , 0.2f));
            }
        }
    }
    public void OnCollisionEnter(Collision collision)
    {
        // ロープへの登録
        if (collision.gameObject.CompareTag("Scrap"))
        {
            BaseScrap scrap = collision.gameObject.GetComponent<BaseScrap>();
            if (scrap.scrapState != BaseScrap.ScrapState.Tethered)
            {
                scrap.scrapState = BaseScrap.ScrapState.Tethered;
                towList.Add(new RopeElement{
                    gameObj = collision.gameObject,
                    rb = collision.gameObject.GetComponent<Rigidbody>()
                });
            }
        }
    }
    public void ThrowingScrap(int removeIndex)
    {
        // ロープからの削除、投擲
        if (towList.Count <= 0)
        {
            return;
        }
        // 投擲
        GameObject removeObj = towList[removeIndex].gameObj;
        Rigidbody removeObjRb = towList[removeIndex].rb;
        removeObjRb.AddForce(
            ThrowVectorGetter.CalculateLaunchVectorWithApexHeight(
                removeObj.transform.position,
                mouseWorldPointer.GetLastPosOrDefault().point,
                2.0f,
                out requiredSpeed
            ),
            ForceMode.VelocityChange
        );
        // 投げたもののStateを変更
        removeObj.GetComponent<BaseScrap>().scrapState = BaseScrap.ScrapState.InFlight;

        // ロープの管理下から外す。
        towList.RemoveAt(removeIndex);

    }

    public void MargeThorowScrap()
    {
        // Scrapの合体
        if (towList.Count < 2)
        {
            return;
        }
        RopeElement listZeroObj = towList[0];
        if (listZeroObj.gameObj.GetComponent<MergedScrap>() == null)
        {
            GameObject mergedScrapObj = Instantiate(mergedScrapPrefab);

            listZeroObj.gameObj.transform.parent = mergedScrapObj.transform;
            listZeroObj.rb.isKinematic = true;
            System.Array.ForEach(listZeroObj.gameObj.GetComponents<Collider>(), c => c.enabled = false);
            towList[0] = new RopeElement { gameObj = mergedScrapObj, rb = mergedScrapObj.GetComponent<Rigidbody>() };
            listZeroObj.gameObj.transform.localPosition = new Vector3(Random.Range(-0.2f,0.2f), Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
            listZeroObj.gameObj.transform.LookAt(towList[0].gameObj.transform);

            towList[0].rb.mass += listZeroObj.rb.mass;

            // radius、Mass
        }
        towList[1].gameObj.GetComponent<Rigidbody>().isKinematic = true;
        System.Array.ForEach(towList[1].gameObj.GetComponents<Collider>(), c => c.enabled = false);
        towList[1].gameObj.transform.parent = towList[0].gameObj.transform;
        towList[1].gameObj.transform.localPosition = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
        towList[1].gameObj.transform.LookAt(towList[0].gameObj.transform);
        towList[0].rb.mass += towList[1].rb.mass;
        towList.RemoveAt(1);
    }

}
