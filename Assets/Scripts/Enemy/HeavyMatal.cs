using UnityEngine;

public class HeavyMatal : BaseEnemy
{
    [SerializeField]private GameObject atkParticle;
    protected override void Update()
    {
        base.Update();
    }

    public override void Attack()
    {
        base.Attack();
        Instantiate(atkParticle, transform.position, Quaternion.identity);
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
