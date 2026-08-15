using System;
using System.Collections.Generic;
using UnityEngine;

public class ClearObjective : MonoBehaviour
{
    // 壊れた飛行船
    public bool[] repairList = new bool[] {false,false,false};
    [SerializeField]private MeshRenderer repairMeshA;
    [SerializeField]private MeshRenderer repairMeshB;
    [SerializeField]private MeshRenderer repairMeshC;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.StartsWith("Scrap/Parts"))
        {
            BaseScrap bS = collision.gameObject.GetComponent<BaseScrap>();

            if (!(bS.scrapState == BaseScrap.ScrapState.InFlight || bS.scrapState == BaseScrap.ScrapState.Usually))
            {
                return;
            }
            switch (collision.gameObject.tag)
            {
                case "Scrap/Parts/A":
                    repairList[0] = true;
                    Destroy(collision.gameObject);
                    repairMeshA.enabled = true;
                    break;
                
                case "Scrap/Parts/B":
                    repairList[1] = true;
                    Destroy(collision.gameObject);
                    repairMeshB.enabled = true;

                    break;
                
                case "Scrap/Parts/C":
                    repairList[2] = true;
                    Destroy(collision.gameObject);
                    repairMeshC.enabled = true;
                    
                    break;
                
                default:
                    break;
            }

            if (CheckrepairList())
            {
                GameManager.ChangeGameState(GameManager.GameState.Clear);
            }
        }
    }

    // クリアしたかどうかの確認
    private bool CheckrepairList()
    {
        foreach (var element in repairList)
        {
            if (element == true)
            {
                continue;
            }
            else
            {
                return false;
            }

        }
        return true;
    }
}
