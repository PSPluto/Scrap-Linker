using UnityEngine;

public class HeavyMatal : BaseEnemy
{
    protected override void Update()
    {
        base.Update();
    }

    public override void Attack()
    {
        base.Attack();
        Debug.Log ("HeavyMatal Attack");
    }
    
    public override void EndAttack()
    {
        base.EndAttack();
    }

    public override void TakeDamage(float damageAmount)
    {
        base.TakeDamage(damageAmount);
    }
}
