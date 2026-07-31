using System.Collections;
using UnityEngine;

public class NitroGenOxides : BaseEnemy
{
    private Coroutine shotCoroutine = null;
    [SerializeField] private GameObject boltPrefab; 
    [Header("弾の性能")]
    [SerializeField] private float boltSpeed = 4;

    [SerializeField] private float ShotInterval = 0.2f;
    protected override void UpdateAttackState()
    {
        if (lastState != state)
        {
            if (shotCoroutine == null)
            {
                shotCoroutine = StartCoroutine(Shot(10));
            }
        }
        base.UpdateAttackState();
    }

    IEnumerator Shot(int shotCount)
    {
        while (true)
        {
            if (shotCount <= 0)
            {
                shotCoroutine = null;
                ChangeState(EnemyState.Chase);
                yield break;
            }
            shotCount--; 
            //発射
            GameObject boltObj = Instantiate(boltPrefab, transform.position + transform.forward + new Vector3(0,0.75f,0), transform.rotation);
            boltObj.GetComponent<Rigidbody>().AddForce(transform.forward * boltSpeed , ForceMode.VelocityChange);
            yield return new WaitForSeconds(ShotInterval);
        }
    }
}