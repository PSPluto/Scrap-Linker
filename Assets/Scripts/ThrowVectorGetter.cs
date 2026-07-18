using UnityEngine;

public static class ThrowVectorGetter
{
    /// <summary>
    /// 頂点の高さを指定して、常に一定の「山なり感」の発射ベクトルを計算する
    /// </summary>
    /// <param name="startPos">発射位置</param>
    /// <param name="targetPos">着地点</param>
    /// <param name="apexHeight">発射位置を基準にした頂点の高さ（例: 2.0なら2m盛り上がる）</param>
    /// <param name="requiredSpeed">計算結果として必要になる速度（out引数）</param>
    /// <returns>AddForce(ForceMode.VelocityChange)にそのまま渡せる速度ベクトル</returns>
    public static Vector3 CalculateLaunchVectorWithApexHeight(
        Vector3 startPos, Vector3 targetPos, float apexHeight, out float requiredSpeed)
    {
        Vector3 diff = targetPos - startPos;
        Vector3 horizontalDir = new Vector3(diff.x, 0f, diff.z);
        float x = horizontalDir.magnitude; // 水平距離
        float y = diff.y;                   // 高低差（ターゲットが高いほど+）

        float g = Mathf.Abs(Physics.gravity.y);
        if (g < 0.0001f) g = 9.81f; // 重力が0設定の場合の保険

        // ターゲットが指定した頂点より高い位置にある場合、頂点を必ずそれより上に補正する
        // （でないと「落ちながら上るターゲット」に届かない矛盾が起きるため）
        float safeApex = Mathf.Max(apexHeight, y + 0.01f);

        // 真上・真下（水平距離がほぼ0）は山なりが作れないので垂直に打ち上げる
        if (x < 0.001f)
        {
            float vy0Vertical = Mathf.Sqrt(2f * g * safeApex);
            requiredSpeed = vy0Vertical;
            return Vector3.up * vy0Vertical;
        }

        horizontalDir.Normalize();

        // 頂点までの上昇時間
        float timeUp = Mathf.Sqrt(2f * safeApex / g);
        // 頂点からターゲットの高さまで落下する時間
        float fallHeight = safeApex - y;
        float timeDown = Mathf.Sqrt(2f * fallHeight / g);

        float totalTime = timeUp + timeDown;
        if (totalTime < 0.0001f) totalTime = 0.0001f; // ゼロ割防止

        // 初速の垂直・水平成分
        float vy0 = g * timeUp;
        float vx0 = x / totalTime;

        requiredSpeed = Mathf.Sqrt(vx0 * vx0 + vy0 * vy0);

        return horizontalDir * vx0 + Vector3.up * vy0;
    }
}