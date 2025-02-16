using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class RangedAttackRadius : AttackRadius
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private BulletEnemie bulletPrefab;
    [SerializeField] private Vector3 bulletSpawnOffset = new Vector3(0, 1, 0);
    [SerializeField] private LayerMask mask;
    [SerializeField] private ObjectPool bulletPool;
    [SerializeField] private float sphereCastRadius = 0.1f;

    private RaycastHit hit;
    private IDamageable targetDamageable;
    private BulletEnemie bullet;

    protected override void Awake()
    {
        base.Awake();

        bulletPool = ObjectPool.CreateInstance(bulletPrefab, Mathf.CeilToInt(1 / attackDelay * bulletPrefab.AutoDestroyTime));
        agent = GetComponentInParent<NavMeshAgent>();
    }

    protected override IEnumerator Attack()
    {
        WaitForSeconds wait = new WaitForSeconds(AttackDelay);

        yield return wait;

        while(damageables.Count > 0){
            for (int i = 0; i < damageables.Count; i++)
            {
                if(HasLineOfSight(damageables[i].GetTransform())){
                    targetDamageable = damageables[i];
                    OnAttack?.Invoke(damageables[i]);
                    agent.enabled = false;
                    break;
                }
            }

            if (targetDamageable != null){
                PoolableObject poolableObject = bulletPool.GetObject();
                if(poolableObject != null){
                    bullet = poolableObject.GetComponent<BulletEnemie>();

                    bullet.Damage = damage;
                    bullet.transform.position = transform.position + bulletSpawnOffset;
                    bullet.transform.rotation = agent.transform.rotation;
                    bullet.Rb.AddForce(agent.transform.forward * bulletPrefab.MoveSpeed, ForceMode.VelocityChange);
                }
            }
            else{
                agent.enabled = true; //No hubo vision del jugador, seguir acercandose
            }

            yield return wait;

            if(targetDamageable == null || !HasLineOfSight(targetDamageable.GetTransform())){
                agent.enabled = true;
            }

            damageables.RemoveAll(DisabledDamageables);
        }

        agent.enabled = true;
        attackCoroutine = null;
    }

    private bool HasLineOfSight(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;
        direction.Normalize();

        if(Physics.SphereCast(transform.position + bulletSpawnOffset, sphereCastRadius, (target.position + bulletSpawnOffset) - (transform.position + bulletSpawnOffset).normalized, out hit, sphereCollider.radius, mask)){
            IDamageable damageable;
            if(hit.collider.TryGetComponent<IDamageable>(out damageable)){
                return damageable.GetTransform() == target;
            }
        }

        return false;
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        if(attackCoroutine == null){
            agent.enabled = true;
        }
    }

}
