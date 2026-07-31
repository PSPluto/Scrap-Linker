using UnityEngine;

public class EnemyTemp : BaseEnemy
{
    protected override void Update()
    {
        // 条件によるStateの遷移とそのStateでの動作を行っている。
        base.Update();
    }

    public override void Attack()
    {
        // ダメージ与処理
        base.Attack();
    }

    public override void EndAttack()
    {
        // 攻撃終了の処理。基本的にAnimationEventから呼ばれる形
        base.EndAttack();
    }

    public override void TakeDamage(float damageAmount)
    {
        // 被ダメージ処理
        base.TakeDamage(damageAmount);
    }
}