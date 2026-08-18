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
    [SerializeField] private AudioClip joinSE;
    [SerializeField] private AudioClip margeSE;

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
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            isLeftClicked = true;
        }
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            isRightClicked = true;
        }

        if (Gamepad.current != null)
        {
            // ZR：投げる
            if (Gamepad.current.rightTrigger.wasPressedThisFrame)
            {
                isLeftClicked = true;
            }
            // ZL：まとめる
            if (Gamepad.current.leftTrigger.wasPressedThisFrame)
            {
                isRightClicked = true;
            }
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
                

                Vector3 playerVelocity = PlayerController.Instance.playerRb.linearVelocity;
                if (playerVelocity.y < 0f)
                {
                    BaseScrap scrap = towList[i].gameObj.GetComponent<BaseScrap>();
                    Vector3 result = new Vector3(playerVelocity.x, playerVelocity.y /scrap.floatPower, playerVelocity.z);
                    PlayerController.Instance.playerRb.linearVelocity = result;
                }
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
        // 「Scrap/」で始まる下位階層のタグ、または「Scrap」完全一致であるかを判定
        // （※もしScrapそのもののタグは除外して、下位だけを対象にしたい場合は、後半の「|| tag == "Scrap"」を消してください）
        string tag = collision.gameObject.tag;
        if (!(tag.StartsWith("Scrap/") || tag == "Scrap")) return;

        BaseScrap scrap = collision.gameObject.GetComponent<BaseScrap>();
    
        // コンポーネントが取得できなかった場合の安全対策
        if (scrap == null) return;

        if (scrap.scrapState != BaseScrap.ScrapState.Tethered)
        {
            scrap.scrapState = BaseScrap.ScrapState.Tethered;
            towList.Add(new RopeElement{
                gameObj = collision.gameObject,
                rb = collision.gameObject.GetComponent<Rigidbody>(),
                scrapScript = scrap
            });
            AudioManager.Instance.PlaySound(joinSE,collision.transform.position);
        }
    }


    public void ThrowingScrap(int removeIndex , bool isDrop = false)
    {
        // ロープからの削除、投擲
        if (towList.Count <= 0)
        {
            return;
        }
        // 投擲
        RopeElement removeElement = towList[removeIndex];
        if (isDrop == false)
        {
            removeElement.rb.AddForce(
                ThrowVectorGetter.CalculateLaunchVectorWithApexHeight(
                    removeElement.gameObj.transform.position,
                    mouseWorldPointer.GetLastPosOrDefault().point,
                    2.0f,
                    out requiredSpeed
                ),
                ForceMode.VelocityChange
            );
        }
        else
        {
            removeElement.rb.AddForce(new Vector3(Random.Range(-0.2f, 0.2f), 2, Random.Range(-0.2f, 0.2f)));
        }
        // Stateを変更
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
            AddDamageStats(towList[0].scrapScript, listZeroObj.scrapScript);
        }
        
        towList[1].scrapScript.isMerged = true;
        // 性能受け渡し
        towList[1].gameObj.transform.parent = towList[0].gameObj.transform;
        
        //見ため
        towList[1].gameObj.transform.localPosition = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
        towList[1].gameObj.transform.LookAt(towList[0].gameObj.transform);
        towList[0].scrapScript.mass += towList[1].scrapScript.mass;
        AddDamageStats(towList[0].scrapScript, towList[1].scrapScript);
        
        towList.RemoveAt(1);
        
        AudioManager.Instance.PlaySound(margeSE,this.transform.position);
    }

    /// <summary>
    /// マージ時にステータスをtargetへ加算する
    /// </summary>
    private void AddDamageStats(BaseScrap target, BaseScrap source)
    {
        target.weakDamage += source.weakDamage;
        target.baseDamage += source.baseDamage;
        target.criticalDamage += source.criticalDamage;
    }
    
    public void dropAllScrap()
    {
        // 要素がなくなるまで常に先頭を削除する
        while (towList.Count > 0)
        {
            ThrowingScrap(0, true);
        }
    }

}