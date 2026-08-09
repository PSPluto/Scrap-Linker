using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterArea : MonoBehaviour
{
    [Header("浮力設定")]
    [Tooltip("浮力の全体的な強さ係数。Buoyancy.upForce に掛け合わされる")]
    [SerializeField] private float buoyancyStrength = 10f;

    [Header("水中抵抗(減衰)")]
    [Tooltip("水中にいる間の線形ドラッグ(跳ねすぎ・沈みすぎ防止)")]
    [SerializeField] private float waterLinearDrag = 3f;
    [Tooltip("水中にいる間の角速度ドラッグ(回転を落ち着かせる)")]
    [SerializeField] private float waterAngularDrag = 1f;

    [Header("姿勢安定化(任意)")]
    [Tooltip("水面で自然に水平姿勢へ戻ろうとするトルクの強さ。0で無効")]
    [SerializeField] private float uprightTorque = 2f;

    [Header("波(任意)")]
    [SerializeField] private bool useWave = false;
    [SerializeField] private float waveHeight = 0.3f;
    [SerializeField] private float waveSpeed = 1f;
    [SerializeField] private float waveScale = 0.5f;
    
    private Collider waterCollider;

    private void Awake()
    {
        waterCollider = GetComponent<Collider>();
    }

    private class SubmergedBody
    {
        public Rigidbody rb;
        public Collider col;
        public float originalLinearDrag;
        public float originalAngularDrag;
    }

    private readonly Dictionary<Buoyancy, SubmergedBody> submergedBodies = new Dictionary<Buoyancy, SubmergedBody>();

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[WaterArea] Trigger Enter: {other.gameObject.name}");

        if (!other.TryGetComponent<Buoyancy>(out Buoyancy buoy))
        {
            Debug.LogWarning($"[WaterArea] {other.gameObject.name} に Buoyancy コンポーネントがありません");
            return;
        }

        if (buoy.rb == null)
        {
            Debug.LogWarning($"[WaterArea] {other.gameObject.name} の Buoyancy.rb が null です");
            return;
        }

        Debug.Log($"[WaterArea] {other.gameObject.name} を浸水リストに追加しました");

        submergedBodies[buoy] = new SubmergedBody
        {
            rb = buoy.rb,
            col = other,
            originalLinearDrag = buoy.rb.linearDamping,
            originalAngularDrag = buoy.rb.angularDamping
        };
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<Buoyancy>(out Buoyancy buoy)) return;

        if (submergedBodies.TryGetValue(buoy, out var body) && body.rb != null)
        {
            body.rb.linearDamping = body.originalLinearDrag;
            body.rb.angularDamping = body.originalAngularDrag;
        }
        submergedBodies.Remove(buoy);
    }
    private float GetWaterSurfaceY(Vector3 worldPos)
    {
        float baseY = waterCollider != null ? waterCollider.bounds.max.y : transform.position.y;
        if (!useWave) return baseY;

        float wave = Mathf.Sin(Time.time * waveSpeed + (worldPos.x + worldPos.z) * waveScale) * waveHeight;
        return baseY + wave;
    }

    private void FixedUpdate()
    {
        if (submergedBodies.Count == 0) return;

        // 列挙中に辞書を変更する可能性があるのでキーをコピー
        List<Buoyancy> keys = new List<Buoyancy>(submergedBodies.Keys);

        foreach (var buoy in keys)
        {
            if (buoy == null || !submergedBodies.TryGetValue(buoy, out SubmergedBody body) || body.rb == null || body.col == null)
            {
                submergedBodies.Remove(buoy);
                continue;
            }
            Debug.Log($"[WaterArea] 浮力計算中: {buoy.name}");

            Rigidbody rb = body.rb;
            Bounds bounds = body.col.bounds;
            float objectHeight = Mathf.Max(bounds.size.y, 0.01f);

            float waterY = GetWaterSurfaceY(bounds.center);
            float bottomY = bounds.min.y;
            float topY = bounds.max.y;

            // 浸水率(0=完全に水面上, 1=完全に水没)
            float submersion = Mathf.InverseLerp(bottomY, topY, waterY);
            submersion = Mathf.Clamp01(submersion);

            Debug.Log($"[WaterArea] {buoy.name} waterY={waterY:F2} bottomY={bottomY:F2} topY={topY:F2} submersion={submersion:F2}");

            if (submersion <= 0f)
            {
                rb.linearDamping = body.originalLinearDrag;
                rb.angularDamping = body.originalAngularDrag;
                continue;
            }

            // 浮力(浸水率に比例。完全に沈んだ状態でBuoyancy.upForce相当の力)
            float force = buoyancyStrength * buoy.upForce * submersion;
            Debug.Log($"[WaterArea] {buoy.name} submersion={submersion:F2} force={force:F1} mass={rb.mass} isKinematic={rb.isKinematic} velocity={rb.linearVelocity}");
            rb.AddForceAtPosition(Vector3.up * force, bounds.center, ForceMode.Force);

            // 水中抵抗を浸水率に応じて滑らかに適用
            rb.linearDamping = Mathf.Lerp(body.originalLinearDrag, waterLinearDrag, submersion);
            rb.angularDamping = Mathf.Lerp(body.originalAngularDrag, waterAngularDrag, submersion);

            // 水面付近で自然に水平姿勢へ戻ろうとするトルク(任意)
            if (uprightTorque > 0f)
            {
                Vector3 currentUp = rb.transform.up;
                Vector3 torqueAxis = Vector3.Cross(currentUp, Vector3.up);
                rb.AddTorque(torqueAxis * uprightTorque * submersion, ForceMode.Force);
            }
            
        }
    }
}