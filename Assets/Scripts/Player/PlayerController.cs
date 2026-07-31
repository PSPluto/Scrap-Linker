using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using static UnityEngine.Rendering.DebugUI;
using RangeAttribute = UnityEngine.RangeAttribute;

public class PlayerController : MonoBehaviour , IDamageable
{
    public static PlayerController Instance{get; private set;}
    
    private static readonly int Speed = Animator.StringToHash("Speed");
    [FormerlySerializedAs("playerRB")] public Rigidbody playerRb;
    public float maxMoveSpeed = 10f;
    public Animator playerAnimator;

    [Range(0.01f, 1f)]
    public float accelerationSmooth = 0.1f;

    [Header("回転設定")]
    public float rotationSpeed = 720f; // deg/sec

    [Header("停止判定")]
    [SerializeField] private float stopThreshold = 0.05f;

    [SerializeField] private Vector3 currentVelocity;

    [FormerlySerializedAs("maxHP")] [Header("HP")]
    public float maxHp = 10;
    private float _currentHp;

    [Header("移動速度の低下倍率")] public float deBuffSpeedMultiplier = 0.8f;
    [Header("移動速度の残りデバフ時間")] public float debuffTime = 0f;

    // WASD入力値
    private Vector2 _value = Vector2.zero;

    private void Awake()
    {
        PlayerController.Instance = this;
    }

    private void Start()
    {
        _currentHp = maxHp;
    }

    private void Update()
    {
        _value = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) _value.y = 1f;
            if (Keyboard.current.sKey.isPressed) _value.y = -1f;
            if (Keyboard.current.aKey.isPressed) _value.x = -1f;
            if (Keyboard.current.dKey.isPressed) _value.x = 1f;
        }

        if (_value.magnitude > 1f)
        {
            _value.Normalize();
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
        Vector3 targetVelocity = new Vector3(_value.x * maxMoveSpeed, 0f, _value.y * maxMoveSpeed) * (1f - (1f - deBuffSpeedMultiplier) * (float)System.Convert.ToInt32(debuffTime > 0f));

        // 現在の速度を取得（y無視）
        currentVelocity = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);
        
        
        if (debuffTime > 0f)
        {
            debuffTime -= Time.fixedDeltaTime;
        }
        if (debuffTime <= 0f)
        {
            debuffTime = 0f;
        }
        playerAnimator.SetFloat(Speed, currentVelocity.magnitude);


        // 最高速度まで速度加算
        Vector3 nextVelocity = Vector3.Lerp(currentVelocity, targetVelocity, accelerationSmooth);

        if (targetVelocity.sqrMagnitude < 0.0001f && nextVelocity.magnitude < stopThreshold)
        {
            nextVelocity = Vector3.zero;
        }

        // 次の速度にするための速度変化量を計算
        Vector3 velocityChange = nextVelocity - currentVelocity;

        // 適用
        playerRb.AddForce(velocityChange, ForceMode.VelocityChange);

        // 移動方向へ向く
        Vector3 moveDir = new Vector3(_value.x, 0f, _value.y);
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            playerRb.MoveRotation(
                Quaternion.RotateTowards(playerRb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime)
            );
        }


        //playerRB.rotation = Quaternion.Lerp(playerRB.rotation, Quaternion.Euler(0, 0, 0),0.2f);
    }

    public void TakeDamage(float damageAmount)
    {
        _currentHp -= damageAmount;
        Debug.Log($"Player HP: {_currentHp}/{maxHp}");
        if (_currentHp <= 0)
        {
            Debug.Log("Player is dead!");
            // ゲームオーバー処理などをここに追加
        }
    }
}
