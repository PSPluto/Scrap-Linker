using UnityEngine;
using UnityEngine.InputSystem;

public class MouseWorldPointer : MonoBehaviour
{
    private RaycastHit lastCastHit;

    [Header("コントローラー設定")]
    [Tooltip("右スティックのデッドゾーン")]
    [Range(0f, 0.9f)]
    public float stickDeadzone = 0.15f;
    [Tooltip("右スティックで仮想カーソルを動かす速度（画面ピクセル/秒）")]
    public float stickCursorSpeed = 1500f;

    [Header("カーソル表示")]
    [Tooltip("画面に表示する仮想カーソルのUI（Canvas配下のImage等のRectTransform）")]
    public RectTransform cursorUI;
    [Tooltip("表示に使うCanvas。Screen Space - OverlayならNullのままでOK。Screen Space - Cameraの場合は指定してください")]
    public Canvas parentCanvas;
    [Tooltip("マウス操作中もカーソルUIを表示するか（falseならコントローラー操作時のみ表示）")]
    public bool showOnMouseInput = false;

    private bool _lastInputWasGamepad = false;

    // 仮想カーソル位置（コントローラー操作用）
    private Vector2 _virtualCursorPos;
    private bool _virtualCursorInitialized = false;

    // マウス座標に向かってレイを飛ばし、ヒットした座標を返す
    public Vector3? Raycast()
    {
        Vector2 screenPos = GetPointerScreenPosition();
        UpdateCursorUI(screenPos);

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            lastCastHit = hit;
            return hit.point;
        }
        return null;
    }

    // カーソルUIの位置と表示状態を更新する
    private void UpdateCursorUI(Vector2 screenPos)
    {
        if (cursorUI == null)
        {
            return;
        }

        bool shouldShow = _lastInputWasGamepad || showOnMouseInput;
        if (cursorUI.gameObject.activeSelf != shouldShow)
        {
            cursorUI.gameObject.SetActive(shouldShow);
        }

        if (!shouldShow)
        {
            return;
        }

        if (parentCanvas == null || parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // Screen Space - Overlayの場合はスクリーン座標をそのまま使える
            cursorUI.position = screenPos;
        }
        else
        {
            // Screen Space - Camera / World Spaceの場合はローカル座標に変換
            Camera cam = parentCanvas.renderMode == RenderMode.ScreenSpaceCamera ? parentCanvas.worldCamera : Camera.main;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvas.transform as RectTransform, screenPos, cam, out Vector2 localPoint))
            {
                cursorUI.localPosition = localPoint;
            }
        }
    }

    // マウス／右スティックの入力に応じて画面座標を返す
    private Vector2 GetPointerScreenPosition()
    {
        if (!_virtualCursorInitialized)
        {
            // 初回はマウス位置（無ければ画面中央）で仮想カーソルを初期化
            _virtualCursorPos = Mouse.current != null
                ? Mouse.current.position.ReadValue()
                : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            _virtualCursorInitialized = true;
        }

        Vector2 stickInput = Vector2.zero;
        if (Gamepad.current != null)
        {
            stickInput = Gamepad.current.rightStick.ReadValue();
            if (stickInput.magnitude < stickDeadzone)
            {
                stickInput = Vector2.zero;
            }
        }

        if (stickInput != Vector2.zero)
        {
            // スティック入力があれば仮想カーソルを移動させる（画面端でクランプ）
            _virtualCursorPos += stickInput * stickCursorSpeed * Time.deltaTime;
            _virtualCursorPos.x = Mathf.Clamp(_virtualCursorPos.x, 0f, Screen.width);
            _virtualCursorPos.y = Mathf.Clamp(_virtualCursorPos.y, 0f, Screen.height);
            _lastInputWasGamepad = true;
        }
        else if (Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero)
        {
            // マウスが動いていればマウス優先で追従させる
            _virtualCursorPos = Mouse.current.position.ReadValue();
            _lastInputWasGamepad = false;
        }

        return _virtualCursorPos;
    }

    // 1行で関数を宣言できる。returnもかける（ここではクラスで宣言されたメソッド外の変数を返してる。）。
    public RaycastHit GetLastPosOrDefault() => lastCastHit;
}