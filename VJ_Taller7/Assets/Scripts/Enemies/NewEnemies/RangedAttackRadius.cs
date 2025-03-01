using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RangedAttackRadius : AttackRadius
{
    [Header("Ranged Attack Settings")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private BulletEnemie bulletPrefab;
    public BulletEnemie BulletPrefab{ get => bulletPrefab; set => bulletPrefab = value; }

    [SerializeField] private Vector3 bulletSpawnOffset = new Vector3(0, 1, 0);
    public Vector3 BulletSpawnOffset{ get => bulletSpawnOffset; set => bulletSpawnOffset = value; }

    [SerializeField] private LayerMask mask;
    public LayerMask Mask{ get => mask; set => mask = value; }

    [SerializeField] private ObjectPool bulletPool;
    [SerializeField] private float sphereCastRadius = 0.1f;

    [Header("Enemy Settings")]
    [SerializeField] private Enemy enemy;

    private RaycastHit hit;
    private IDamageable targetDamageable;
    private BulletEnemie bullet;

    protected override void Awake()
    {
        base.Awake();

        agent = GetComponentInParent<NavMeshAgent>();
    }

    public void CreateBulletPool(){
        if (bulletPool == null){
            bulletPool = ObjectPool.CreateInstance(bulletPrefab, Mathf.CeilToInt(1 / attackDelay * bulletPrefab.AutoDestroyTime));
        }
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

                    if(!enemy.IsStatic)
                    agent.isStopped = true;
                    break;
                }
            }

            if (targetDamageable != null){
                PoolableObject poolableObject = bulletPool.GetObject();
                if(poolableObject != null){
                    bullet = poolableObject.GetComponent<BulletEnemie>();

                    bullet.transform.position = transform.position + bulletSpawnOffset;
                    bullet.transform.rotation = agent.transform.rotation;

                    bullet.Spawn(agent.transform.forward, damage, targetDamageable.GetTransform());
                }
            }
            else{
                if(!enemy.IsStatic)
                agent.isStopped = false; //No hubo vision del jugador, seguir acercandose
            }

            yield return wait;

            if(targetDamageable == null || !HasLineOfSight(targetDamageable.GetTransform())){
                if(!enemy.IsStatic)
                agent.isStopped = false;
            }

            damageables.RemoveAll(DisabledDamageables);
        }

        if(!enemy.IsStatic)
        agent.isStopped = false;

        attackCoroutine = null;
    }

    private bool HasLineOfSight(Transform target)
    {
        Vector3 origin = transform.position + bulletSpawnOffset;
        Vector3 direction = (target.position + bulletSpawnOffset - origin).normalized;
        float distance = Vector3.Distance(origin, target.position + bulletSpawnOffset);

        Debug.DrawRay(origin, direction, Color.red, 1.0f);

        if (Physics.SphereCast(origin, sphereCastRadius, direction, out hit, distance, mask)){
            IDamageable damageable;
            if(hit.collider.TryGetComponent<IDamageable>(out damageable)){
                Debug.Log("Line of sight to target: " + (damageable.GetTransform() == target));
                return damageable.GetTransform() == target;
            }
            else
            {
                Debug.Log($"Hit: {hit.collider.name}, but it is not damageable");
            }   
        }
        else
        {
            Debug.Log("SphereCast did not hit anything");
        }

        Debug.Log("No line of sight to target");
        return false;
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        if(attackCoroutine == null){
            if(!enemy.IsStatic)
            agent.enabled = true;
        }
    }

}
