using System;
using System.Net;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using static UnityEngine.Rendering.DebugUI;
using RangeAttribute = UnityEngine.RangeAttribute;

public class PlayerController : MonoBehaviour , IDamageable
{
    [SerializeField]private Vector3 defaultRespawnPosition;
    public Vector3 respawnPos = new Vector3(0, 0, 0);
    public static PlayerController Instance{get; private set;}
    
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int JumpTrigger = Animator.StringToHash("Jump");
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");

    public Rigidbody playerRb;
    public float maxMoveSpeed = 10f;
    public Animator playerAnimator;

    [Range(0.01f, 1f)]
    public float accelerationSmooth = 0.5f;

    [Header("空中制御（ジャンプ弱体化用）")]
    [Tooltip("空中での移動速度倍率。1で地上と同じ、小さいほど空中移動が弱くなる")]
    [Range(0.01f, 1f)]
    public float airControlMultiplier = 0.5f;
    [Tooltip("空中での加速の滑らかさ。小さいほど切り返しが鈍くなり、慣性が強く残る")]
    [Range(0.01f, 1f)]
    public float airAccelerationSmooth = 0.2f;

    [Header("回転設定")]
    public float rotationSpeed = 720f; // deg/sec

    [Header("停止判定")]
    [SerializeField] private float stopThreshold = 0.05f;

    [SerializeField] private Vector3 currentVelocity;

    [Header("ジャンプ設定")]
    public float jumpForce = 6f;
    [Tooltip("レイの発射高")]
    public float groundCheckOriginHeight = 0.1f;
    [Tooltip("飛ばす距離")]
    public float groundCheckDistance = 0.3f;
    [Tooltip("接地判定の球体の半径")]
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer = ~0;
    public float jumpCooldown = 0.1f;

    [Header("着地硬直（ジャンプ弱体化用）")]
    [Tooltip("着地した瞬間から、この時間だけ移動加速をさらに鈍らせる")]
    public float landingRecoverTime = 0.15f;
    [Tooltip("着地硬直中の移動速度倍率")]
    [Range(0.01f, 1f)]
    public float landingSpeedMultiplier = 0.5f;

    private bool _isGrounded;
    private bool _wasGroundedLastFixedUpdate = true;
    private bool _jumpQueued;
    private float _lastJumpTime = -999f;
    private float _landingRecoverTimer = 0f;

    [FormerlySerializedAs("maxHP")] [Header("HP")]
    public float maxHp = 10;
    public float currentHp;

    [Header("移動速度の低下倍率")] public float deBuffSpeedMultiplier = 0.8f;
    [Header("移動速度の残りデバフ時間")] public float debuffTime = 0f;
    
    [Header("音")]
    [SerializeField]private AudioClip damageSound;
    [SerializeField]private AudioClip jumpSound;

    // WASD入力値
    private Vector2 _value = Vector2.zero;

    private void Awake()
    {
        PlayerController.Instance = this;
    }

    private void Start()
    {
        currentHp = maxHp;
    }

    [Header("コントローラー設定")]
    [Tooltip("スティックのデッドゾーン")]
    [Range(0f, 0.9f)]
    public float gamepadDeadzone = 0.15f;

    private void Update()
    {
        _value = Vector2.zero;
        bool anyInputThisFrame = false;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                anyInputThisFrame = true;
            }
            if (Keyboard.current.wKey.isPressed) _value.y = 1f;
            if (Keyboard.current.sKey.isPressed) _value.y = -1f;
            if (Keyboard.current.aKey.isPressed) _value.x = -1f;
            if (Keyboard.current.dKey.isPressed) _value.x = 1f;

            // ジャンプ入力（このフレームで押された時のみ）
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                _jumpQueued = true;
                anyInputThisFrame = true;
            }
        }

        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (stick.magnitude < gamepadDeadzone)
            {
                stick = Vector2.zero;
            }
            else
            {
                // デッドゾーン以降を0-1に再スケール
                stick = stick.normalized * ((stick.magnitude - gamepadDeadzone) / (1f - gamepadDeadzone));
            }

            // キーボード入力が無ければスティック値を採用（両対応・キーボード優先）
            if (_value == Vector2.zero && stick != Vector2.zero)
            {
                _value = stick;
                anyInputThisFrame = true;
            }

            // ジャンプ（Aボタン / Southボタン）
            if (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                _jumpQueued = true;
                anyInputThisFrame = true;
            }
        }

        if (anyInputThisFrame)
        {
            ScoreManager.Instance.StartCount();
        }

        if (_value.magnitude > 1f)
        {
            _value.Normalize();
        }
        if (transform.position.y < -2f)
        {
            TakeDamage(currentHp);
        }
    }
    void FixedUpdate()
    {
        CheckGrounded();
        UpdateLandingRecover();
        Move();
        TryJump();
    }

    private void CheckGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * (groundCheckOriginHeight + groundCheckRadius);
        bool hitGround = Physics.SphereCast(
            origin,
            groundCheckRadius,
            Vector3.down,
            out RaycastHit hit,
            groundCheckOriginHeight + groundCheckDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        _isGrounded = hitGround;

        if (playerAnimator)
        {
            playerAnimator.SetBool(IsGrounded, _isGrounded);
        }
    }

    /// <summary>
    /// 空中→着地の瞬間を検出して、着地硬直タイマーをセットする
    /// </summary>
    private void UpdateLandingRecover()
    {
        if (_isGrounded && !_wasGroundedLastFixedUpdate)
        {
            _landingRecoverTimer = landingRecoverTime;
        }

        if (_landingRecoverTimer > 0f)
        {
            _landingRecoverTimer -= Time.fixedDeltaTime;
            if (_landingRecoverTimer < 0f)
            {
                _landingRecoverTimer = 0f;
            }
        }

        _wasGroundedLastFixedUpdate = _isGrounded;
    }

    private void TryJump()
    {
        if (!_jumpQueued)
        {
            return;
        }

        _jumpQueued = false;

        if (!_isGrounded)
        {
            return;
        }

        if (Time.time - _lastJumpTime < jumpCooldown)
        {
            return;
        }

        _lastJumpTime = Time.time;

        // 上方向の速度をリセットしてから力を加える（連続ジャンプの高さ不安定を防ぐ）
        Vector3 vel = playerRb.linearVelocity;
        vel.y = 0f;
        playerRb.linearVelocity = vel;

        playerRb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(JumpTrigger);
        }

        if (jumpSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(jumpSound, transform.position);
        }
    }
    
    public void Move()
    {
        currentVelocity = Vector3.zero;

        float debuffFactor = 1f - (1f - deBuffSpeedMultiplier) * (float)System.Convert.ToInt32(debuffTime > 0f);

        // 空中では移動速度そのものを制限（歩くより有利にならないようにする）
        float airFactor = _isGrounded ? 1f : airControlMultiplier;

        // 着地直後は硬直として速度をさらに絞る
        float landingFactor = (_isGrounded && _landingRecoverTimer > 0f) ? landingSpeedMultiplier : 1f;

        // 目標速度
        Vector3 targetVelocity = new Vector3(_value.x * maxMoveSpeed, 0f, _value.y * maxMoveSpeed) * (debuffFactor * airFactor * landingFactor);

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

        // 空中では追従を鈍くする（＝慣性が強く残り、急な切り返しができなくなる）
        float smooth = _isGrounded ? accelerationSmooth : airAccelerationSmooth;

        // 最高速度まで速度加算
        Vector3 nextVelocity = Vector3.Lerp(currentVelocity, targetVelocity, smooth);

        if (targetVelocity.sqrMagnitude < 0.0001f && nextVelocity.magnitude < stopThreshold)
        {
            nextVelocity = Vector3.zero;
        }

        // 次の速度にするための速度変化量を計算（Y軸は変更しない＝ジャンプ・重力に干渉しない）
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
    }

    public void TakeDamage(float damageAmount)
    {
        currentHp -= damageAmount;
        AudioManager.Instance.PlaySound(damageSound, transform.position);
        Debug.Log($"Player HP: {currentHp}/{maxHp}");
        if (currentHp <= 0)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        playerRb.isKinematic = true;
        this.gameObject.GetComponent<PlayerRopeManager>().dropAllScrap();
        transform.position = respawnPos;
        currentHp = maxHp;
        playerRb.isKinematic = false;
    }

    private void OnDrawGizmosSelected()
    {
        float radius = groundCheckRadius > 0f ? groundCheckRadius : 0.2f;
        Vector3 origin = transform.position + Vector3.up * (groundCheckOriginHeight + radius);
        float totalDistance = groundCheckOriginHeight + groundCheckDistance;

        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(origin, radius);
        Gizmos.DrawLine(origin, origin + Vector3.down * totalDistance);
        Gizmos.DrawWireSphere(origin + Vector3.down * totalDistance, radius);
    }
}