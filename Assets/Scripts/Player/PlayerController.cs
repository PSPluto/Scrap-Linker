using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;
using RangeAttribute = UnityEngine.RangeAttribute;

public class PlayerController : MonoBehaviour
{
    public Rigidbody playerRB;
    public float maxMoveSpeed = 10f;
    public Animator playerAnimator;

    [Range(0.01f, 1f)]
    public float accelerationSmooth = 0.1f;

    [Header("回転設定")]
    public float rotationSpeed = 720f; // deg/sec

    [Header("停止判定")]
    [SerializeField] private float stopThreshold = 0.05f;

    [SerializeField]private Vector3 currentVelocity;


    // WASD入力値
    private Vector2 value = Vector2.zero;

    private void Update()
    {
        value = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) value.y = 1f;
            if (Keyboard.current.sKey.isPressed) value.y = -1f;
            if (Keyboard.current.aKey.isPressed) value.x = -1f;
            if (Keyboard.current.dKey.isPressed) value.x = 1f;
        }

        if (value.magnitude > 1f)
        {
            value.Normalize();
        }
    }
    void FixedUpdate()
    {
        Move();
    }
    
    public void Move()
    {
        currentVelocity = Vector3.zero;
        // 目標速度
        Vector3 targetVelocity = new Vector3(value.x * maxMoveSpeed, 0f, value.y * maxMoveSpeed);

        // 現在の速度を取得（y無視）
        currentVelocity = new Vector3(playerRB.linearVelocity.x, 0f, playerRB.linearVelocity.z);
        playerAnimator.SetFloat("Speed", currentVelocity.magnitude);


        // 最高速度まで速度加算
        Vector3 nextVelocity = Vector3.Lerp(currentVelocity, targetVelocity, accelerationSmooth);

        if (targetVelocity.sqrMagnitude < 0.0001f && nextVelocity.magnitude < stopThreshold)
        {
            nextVelocity = Vector3.zero;
        }

        // 次の速度にするための速度変化量を計算
        Vector3 velocityChange = nextVelocity - currentVelocity;

        // 適用
        playerRB.AddForce(velocityChange, ForceMode.VelocityChange);

        // 移動方向へ向く
        Vector3 moveDir = new Vector3(value.x, 0f, value.y);
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            playerRB.MoveRotation(
                Quaternion.RotateTowards(playerRB.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime)
            );
        }


        //playerRB.rotation = Quaternion.Lerp(playerRB.rotation, Quaternion.Euler(0, 0, 0),0.2f);
    }
}
