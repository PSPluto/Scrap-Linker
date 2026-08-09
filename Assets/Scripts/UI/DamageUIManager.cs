using System;
using TMPro;
using UnityEngine;

public class DamageUIManager : MonoBehaviour
{
    public static DamageUIManager Instance { get; private set; }

    [SerializeField] private GameObject textPrefab;   // TextMeshProUGUIを子に持つUIプレハブ
    [SerializeField] private Camera worldCamera;       // 3D→スクリーン座標変換に使うカメラ
    [SerializeField] private RectTransform canvasRect; // 生成先のCanvasのRectTransform
    [SerializeField] private Canvas canvas;            // RenderModeの判定に使用

    private void Awake()
    {
        Instance = this;
    }

    public void NewDamageText(float damage, Vector3 worldPos)
    {
        if (textPrefab == null || worldCamera == null || canvasRect == null || canvas == null)
        {
            Debug.LogError(
                $"[DamageUIManager] Inspectorで未設定のフィールドがあります。" +
                $"textPrefab={(textPrefab == null ? "None" : "OK")}, " +
                $"worldCamera={(worldCamera == null ? "None" : "OK")}, " +
                $"canvasRect={(canvasRect == null ? "None" : "OK")}, " +
                $"canvas={(canvas == null ? "None" : "OK")}",
                this
            );
            return;
        }

        // 1. 3Dワールド座標 → スクリーン座標に変換
        Vector3 screenPos = worldCamera.WorldToScreenPoint(worldPos);

        // カメラの後ろにある場合は表示しない
        if (screenPos.z < 0f)
        {
            return;
        }

        // 2. スクリーン座標 → Canvas(RectTransform)内のローカル座標に変換
        Camera uiEventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            uiEventCamera,
            out Vector2 localPoint
        );

        // 3. Canvasの子としてUIオブジェクトを生成
        var textObj = Instantiate(textPrefab, canvasRect);
        var rect = textObj.GetComponent<RectTransform>();
        rect.anchoredPosition = localPoint;

        var tmpText = textObj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText == null)
        {
            Debug.LogError($"[DamageUIManager] textPrefab '{textPrefab.name}' に TextMeshProUGUI が見つかりません。", textObj);
            Destroy(textObj);
            return;
        }
        tmpText.text = damage.ToString();

        Destroy(textObj, 0.8f);
    }
}