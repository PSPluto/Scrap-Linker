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

    // 進捗が変わった時に (現在の修理数, 総数) を通知
    public static event Action<int, int> OnProgressChanged;
    // クリアした時に通知
    public static event Action OnCleared;

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

            // 進捗通知（n/3のnを算出して渡す）
            OnProgressChanged?.Invoke(CountRepaired(), repairList.Length);

            if (CheckrepairList())
            {
                if (GameManager.gameState == GameManager.GameState.GameOver)
                {
                    return;
                }
                GameManager.ChangeGameState(GameManager.GameState.Clear);
                OnCleared?.Invoke();
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

    // 修理済みの数をカウント
    private int CountRepaired()
    {
        int count = 0;
        foreach (var element in repairList)
        {
            if (element) count++;
        }
        return count;
    }
}