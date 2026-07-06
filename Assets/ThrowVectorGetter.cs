using UnityEngine;

public static class ThrowVectorGetter
{
    /// <summary>
    /// スピードを固定し、ターゲットに最も届きやすい発射ベクトルを計算する関数
    /// </summary>
    /// <param name="startPos">発射位置</param>
    /// <param name="targetPos">着地点</param>
    /// <param name="speed">固定したい勢い（スピード）</param>
    /// <returns>AddForceにそのまま渡せるベクトル</returns>
    public static Vector3 CalculateLaunchVectorWithFixedSpeed(Vector3 startPos, Vector3 targetPos, float speed)
    {
        // 水平方向のベクトルと距離を計算
        Vector3 diff = targetPos - startPos;
        Vector3 horizontalDir = new Vector3(diff.x, 0, diff.z);
        float x = horizontalDir.magnitude; // 水平距離
        float y = diff.y;                   // 高低差

        // 真上・真下の場合は、そのまま垂直に飛ばす
        if (x < 0.001f)
        {
            return y > 0 ? Vector3.up * speed : Vector3.down * speed;
        }

        horizontalDir.Normalize(); // 水平方向の単位ベクトル化
        float g = Mathf.Abs(Physics.gravity.y); // 重力（正の数）

        // 物理公式の判別式 (ルートの中身)
        // v^4 - g * (g * x^2 + 2 * y * v^2)
        float v2 = speed * speed;
        float insideRoot = (v2 * v2) - g * (g * (x * x) + 2f * y * v2);

        float tanTheta;

        if (insideRoot >= 0f)
        {
            // 【届く場合】
            // 解は2つ（高い放物線と低い弾道）ありますが、基本的には早く届く「低い弾道」を採用します
            tanTheta = (v2 - Mathf.Sqrt(insideRoot)) / (g * x);
        }
        else
        {
            // 【距離や高さが足りず届かない場合】
            // そのスピードにおいて、最もターゲットに近づく「最大射程」の角度（包絡線）を計算します
            tanTheta = v2 / (g * x);
        }

        // tanから角度（ラジアン）を求める
        float angle = Mathf.Atan(tanTheta);

        // 水平方向と垂直方向の力を合成して、固定スピードのベクトルを作る
        Vector3 launchVelocity = (horizontalDir * Mathf.Cos(angle) * speed) + (Vector3.up * Mathf.Sin(angle) * speed);

        return launchVelocity;
    }
}
