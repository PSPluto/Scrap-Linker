using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static RopeScrapElement;

public class PlayerRopeManager : MonoBehaviour
{
    public MouseWorldPointer mouseWorldPointer;
    [Header("ロープ")] public List<RopeElement> towList = new List<RopeElement>();
    [Header("オブジェクトごとの間隔")]
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

    private void UpdateRope()
    {
        for (int i = 0; i < towList.Count; i++)
        {
            Vector3 direction = Vector3.zero;
            GameObject towObject = towList[i].gameObj;
            float distance = 0f;
            Vector3 newPosition;
            // 持ち上げ状態の処理
            if (i == 0)
            {
                //持ち上げられた状態
                newPosition = this.transform.position + new Vector3(0,0.75f,0);
                towList[i].rb.MovePosition(newPosition);
                towList[i].rb.linearVelocity = new Vector3(0, 0, 0);
                towList[i].rb.MoveRotation(transform.rotation);
                
                if (towList[i].scrapScript.scrapState != BaseScrap.ScrapState.Lifted)
                {
                    towList[i].scrapScript.scrapState = BaseScrap.ScrapState.Lifted;
                }

                continue;
            }
            else
            {
                // ロープにつながった状態の処理
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
        if (!collision.gameObject.CompareTag("Scrap")) return;
        BaseScrap scrap = collision.gameObject.GetComponent<BaseScrap>();
        if (scrap.scrapState != BaseScrap.ScrapState.Tethered)
        {
            scrap.scrapState = BaseScrap.ScrapState.Tethered;
            towList.Add(new RopeElement{
                gameObj = collision.gameObject,
                rb = collision.gameObject.GetComponent<Rigidbody>(),
                scrapScript = scrap
            });
        }
    }

    private void ThrowingScrap(int removeIndex)
    {
        // ロープからの削除、投擲
        if (towList.Count <= 0)
        {
            return;
        }
        // 投擲
        RopeElement removeElement = towList[removeIndex];
        removeElement.rb.AddForce(
            ThrowVectorGetter.CalculateLaunchVectorWithApexHeight(
                removeElement.gameObj.transform.position,
                mouseWorldPointer.GetLastPosOrDefault().point,
                2.0f,
                out requiredSpeed
            ),
            ForceMode.VelocityChange
        );
        // 投げたもののStateを変更
        removeElement.scrapScript.scrapState = BaseScrap.ScrapState.InFlight;

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
            // もし、まとめるための親オブジェクトがないなら
            GameObject mergedScrapObj = Instantiate(mergedScrapPrefab);
            listZeroObj.gameObj.transform.parent = mergedScrapObj.transform;
            //ロープのリスト[0]をscrapをまとめるオブジェクトに置きかえ。
            towList[0] = new RopeElement { gameObj = mergedScrapObj, rb = mergedScrapObj.GetComponent<Rigidbody>(), scrapScript = mergedScrapObj.GetComponent<BaseScrap>() };
            
            // 見た目
            listZeroObj.gameObj.transform.localPosition = new Vector3(Random.Range(-0.2f,0.2f), Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
            listZeroObj.gameObj.transform.LookAt(towList[0].gameObj.transform);
            
            // 通知
            listZeroObj.scrapScript.isMerged = true;
            
            // 性能の受け渡し
            towList[0].scrapScript.mass += listZeroObj.scrapScript.mass;
            towList[0].rb.mass = towList[0].scrapScript.mass;
        }
        
        towList[1].scrapScript.isMerged = true;
        // 性能受け渡し
        towList[1].gameObj.transform.parent = towList[0].gameObj.transform;
        
        //見ため
        towList[1].gameObj.transform.localPosition = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
        towList[1].gameObj.transform.LookAt(towList[0].gameObj.transform);
        towList[0].scrapScript.mass += towList[1].scrapScript.mass;
        
        towList.RemoveAt(1);
    }

}
