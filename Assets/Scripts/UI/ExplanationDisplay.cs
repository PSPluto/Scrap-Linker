using UnityEngine;

// 説明を表示したいオブジェクトにアタッチする。
// マウスがこのオブジェクトに当たると、ExplanationCanvasUI 側が
// この文字列をキャンバスに表示する。
public class ExplanationDisplay : MonoBehaviour
{
    [TextArea(2, 5)]
    [SerializeField] private string explanationText = "ここに説明文を入力";

    public string ExplanationText => explanationText;
}
