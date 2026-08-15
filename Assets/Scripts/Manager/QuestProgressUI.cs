using System.Collections.Generic;
using UnityEngine;
using TMPro; // 標準のUI.Textを使う場合は using UnityEngine.UI; に変更

public class QuestProgressUI : MonoBehaviour
{
    [Header("進捗テキスト（n/3 の部分だけ表示）")]
    [SerializeField] private TextMeshProUGUI progressText;
    // 標準Textを使う場合: [SerializeField] private Text progressText;

    [Header("クリア時に非表示にするオブジェクト一覧")]
    [SerializeField] private List<GameObject> hideList = new List<GameObject>();

    private int totalCount = 3; // 初期表示用（ClearObjectiveから上書きされる）

    private void OnEnable()
    {
        ClearObjective.OnProgressChanged += UpdateProgressText;
        ClearObjective.OnCleared += HideAll;
    }

    private void OnDisable()
    {
        ClearObjective.OnProgressChanged -= UpdateProgressText;
        ClearObjective.OnCleared -= HideAll;
    }

    private void Start()
    {
        // 開始時点で 0/3 を表示
        UpdateProgressText(0, totalCount);
    }

    // n/3 のテキストを更新
    private void UpdateProgressText(int current, int total)
    {
        totalCount = total;
        if (progressText != null)
        {
            progressText.text = $"{current}/{total}";
        }
    }

    // クリア時にhideListのオブジェクトを全て非表示にする
    private void HideAll()
    {
        foreach (var obj in hideList)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }
}