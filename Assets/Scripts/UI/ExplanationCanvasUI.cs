using UnityEngine;
using TMPro;

// MouseWorldPointer と組み合わせて使う。
// マウスの当たったオブジェクトに ExplanationDisplay がついていれば、
// そのワールド座標をキャンバス座標に変換してテキストを表示する。
[RequireComponent(typeof(MouseWorldPointer))]
public class ExplanationCanvasUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private MouseWorldPointer mouseWorldPointer;
    [SerializeField] private Canvas targetCanvas;              // Screen Space - Camera / Overlay どちらでも可
    [SerializeField] private RectTransform explanationPanel;   // 背景+テキストをまとめたパネル
    [SerializeField] private TextMeshProUGUI explanationLabel;
    [SerializeField] private Camera uiCamera;                  // Overlayの場合は空でOK。Screen Space - Cameraなら指定必須

    [Header("表示位置の調整")]
    [SerializeField] private Vector2 screenOffset = new Vector2(20f, 20f);

    private void Awake()
    {
        if (mouseWorldPointer == null)
        {
            mouseWorldPointer = GetComponent<MouseWorldPointer>();
        }

        if (explanationPanel != null)
        {
            explanationPanel.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        Vector3? hitPoint = mouseWorldPointer.Raycast();

        if (hitPoint == null)
        {
            HidePanel();
            return;
        }

        RaycastHit hit = mouseWorldPointer.GetLastPosOrDefault();
        ExplanationDisplay explanation = hit.collider != null
            ? hit.collider.GetComponentInParent<ExplanationDisplay>()
            : null;

        if (explanation == null)
        {
            HidePanel();
            return;
        }

        ShowPanel(explanation.ExplanationText, hitPoint.Value);
    }

    private void ShowPanel(string text, Vector3 worldPos)
    {
        if (explanationPanel == null || explanationLabel == null || targetCanvas == null)
        {
            return;
        }

        explanationPanel.gameObject.SetActive(true);
        explanationLabel.text = text;

        // ワールド座標 → スクリーン座標
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPos);

        // スクリーン座標 → キャンバスのローカル座標
        RectTransform canvasRect = targetCanvas.transform as RectTransform;
        Camera cameraForConversion = targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : uiCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            cameraForConversion,
            out Vector2 canvasLocalPoint
        );

        explanationPanel.anchoredPosition = canvasLocalPoint + screenOffset;
    }

    private void HidePanel()
    {
        if (explanationPanel != null && explanationPanel.gameObject.activeSelf)
        {
            explanationPanel.gameObject.SetActive(false);
        }
    }
}
