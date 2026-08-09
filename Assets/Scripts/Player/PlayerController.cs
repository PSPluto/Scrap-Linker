using System;
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
    public float accelerationSmooth = 0.1f;

    [Header("回転設定")]
    public float rotationSpeed = 720f; // deg/sec

    [Header("停止判定")]
    [SerializeField] private float stopThreshold = 0.05f;

    [SerializeField] private Vector3 currentVelocity;

    [Header("ジャンプ設定")]
    public float jumpForce = 6f;
    [Tooltip("レイの発射高さ（自分のコライダーの外側からスタートさせるため少し上げる）")]
    public float groundCheckOriginHeight = 0.1f;
    [Tooltip("発射点からさらに下に飛ばす距離")]
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer = ~0;
    public float jumpCooldown = 0.1f;

    private bool _isGrounded;
    private bool _jumpQueued;
    private float _lastJumpTime = -999f;

    [FormerlySerializedAs("maxHP")] [Header("HP")]
    public float maxHp = 10;
    private float _currentHp;

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

            // ジャンプ入力（このフレームで押された時のみ）
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                _jumpQueued = true;
            }
        }

        if (_value.magnitude > 1f)
        {
            _value.Normalize();
        }
        if (transform.position.y < -5f)
        {
            TakeDamage(_currentHp);
        }
    }
    void FixedUpdate()
    {
        CheckGrounded();
        Move();
        TryJump();
    }

    private void CheckGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * groundCheckOriginHeight;
        bool hitGround = Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            groundCheckOriginHeight + groundCheckDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        // if (hitGround)
        // {
        //     Debug.Log($"Ground hit: {hit.collider.gameObject.name}, point: {hit.point}, normal: {hit.normal}, distance: {hit.distance}");
        // }

        _isGrounded = hitGround;

        if (playerAnimator != null)
        {
            playerAnimator.SetBool(IsGrounded, _isGrounded);
        }
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


        //playerRB.rotation = Quaternion.Lerp(playerRB.rotation, Quaternion.Euler(0, 0, 0),0.2f);
    }

    public void TakeDamage(float damageAmount)
    {
        _currentHp -= damageAmount;
        AudioManager.Instance.PlaySound(damageSound, transform.position);
        Debug.Log($"Player HP: {_currentHp}/{maxHp}");
        if (_currentHp <= 0)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        playerRb.isKinematic = true;
        this.gameObject.GetComponent<PlayerRopeManager>().dropAllScrap();
        transform.position = respawnPos;
        _currentHp = maxHp;
        playerRb.isKinematic = false;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.up * groundCheckOriginHeight;
        float totalDistance = groundCheckOriginHeight + groundCheckDistance;

        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + Vector3.down * totalDistance);
        Gizmos.DrawWireSphere(origin, 0.03f);
    }
}