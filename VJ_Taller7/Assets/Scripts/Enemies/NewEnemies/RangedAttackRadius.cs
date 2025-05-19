using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RangedAttackRadius : AttackRadius
{
    [Header("Ranged Attack Settings")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private BulletEnemie bulletPrefab;
    public BulletEnemie BulletPrefab{ get => bulletPrefab; set => bulletPrefab = value; }

    [SerializeField] private Transform bulletOrigin;
    public Transform BulletOrigin{ get => bulletOrigin; set => bulletOrigin = value; }

    [SerializeField] private LayerMask mask;
    public LayerMask Mask{ get => mask; set => mask = value; }

    [SerializeField] private ObjectPool bulletPool;
    
    [Tooltip("The radius of the sphere cast used to check for line of sight")]
    [Range(0.01f, 1f)][SerializeField] private float sphereCastRadius = 0.1f;

    private RaycastHit hit;
    private IDamageable targetDamageable;
    private BulletEnemie bullet;
    private Vector3 directionToTarget;

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
        soundManager.PlaySound("Attack");
        enemy.Animator.SetTrigger(Enemy.SHOOT_TRIGGER);

        WaitForSeconds wait = new WaitForSeconds(AttackDelay);

        yield return wait;

        while (damageables.Count > 0){

            if(enemy.IsDead){
                StopAllCoroutines();
                targetDamageable = null;
                damageables.RemoveAll(DisabledDamageables);
                attackCoroutine = null;
                yield break;
            }

            for (int i = 0; i < damageables.Count; i++)
            {
                if(HasLineOfSight(damageables[i].GetTransform())){
                    float distanceToPlayer = Vector3.Distance(enemy.transform.position, damageables[i].GetTransform().position);
                    if (distanceToPlayer < sphereCollider.radius * 0.9f)
                    {
                        enemy.Movement.MoveToAttackDistance(sphereCollider.radius);
                    }
                    if(OnRangeBehvaior == OnRangeBehvaior.Stop){
                        enemy.Movement.StopMovement();
                    }
                    else if (OnRangeBehvaior == OnRangeBehvaior.Follow){
                        enemy.Movement.State = EnemyState.Chase;
                    }
                    else if (OnRangeBehvaior == OnRangeBehvaior.Circles){
                        enemy.Movement.MoveInCircles();
                    }
                    else if (OnRangeBehvaior == OnRangeBehvaior.Around){
                        enemy.Movement.MoveAround();
                    }
                    
                    targetDamageable = damageables[i];
                    OnAttack?.Invoke(damageables[i]);

                    break;
                }
            }

            if (targetDamageable != null){
                PoolableObject poolableObject = bulletPool.GetObject();
                if(poolableObject != null){
                    bullet = poolableObject.GetComponent<BulletEnemie>();

                    bullet.transform.position = bulletOrigin.position;
                    bullet.transform.rotation = agent.transform.rotation;

                    directionToTarget = targetDamageable.GetTransform().position + new Vector3(0, 1, 0) - (bulletOrigin.position + new Vector3(0, 1, 0));
                    bullet.Spawn(directionToTarget + new Vector3(0, 0.5f, 0), damage, targetDamageable.GetTransform());
                }
            }
            else{
                if(!enemy.IsStatic && !agent.enabled){
                    agent.enabled = true;
                }
            }

            yield return wait;

            if(targetDamageable == null || !HasLineOfSight(targetDamageable.GetTransform())){
                if(!enemy.IsStatic){
                    agent.enabled = true;
                }
            }

            damageables.RemoveAll(DisabledDamageables);
        }

        if(!enemy.IsStatic){
            agent.enabled = true;
        }

        attackCoroutine = null;
    }

    private bool HasLineOfSight(Transform target)
    {
        if(enemy.IsDead){
            return false;
        }

        Vector3 origin = bulletOrigin.position;
        Vector3 direction = target.position + new Vector3(0, 1, 0) - (transform.position + new Vector3(0, 1, 0));

        float distance = Vector3.Distance(origin, target.position);

        Debug.DrawLine(origin, origin + direction.normalized * distance, Color.red);
        Debug.DrawRay(origin, direction.normalized * sphereCastRadius, Color.red);

        if (Physics.SphereCast(origin, sphereCastRadius, direction.normalized, out RaycastHit hit, sphereCollider.radius, mask)){
            
            IDamageable damageable;
            
            if(hit.collider.TryGetComponent<IDamageable>(out damageable)){
                //Debug.Log("Line of sight to target: " + (damageable.GetTransform() == target));
                return damageable.GetTransform() == target;
                
            }
            else
            {
                //Debug.Log($"Hit: {hit.collider.name}, but it is not damageable");
            }   
        }
        else
        {
            //Debug.Log("SphereCast did not hit anything");
        }

        //Debug.Log("No line of sight to target");
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

    private void OnDrawGizmosSelected()
    {
        // Draw the bullet spawn position
        Vector3 bulletSpawnPosition = bulletOrigin.position;
        
        // Draw a sphere at spawn position
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(bulletSpawnPosition, 0.2f);
        
        // Draw a line from the center to the spawn point
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, bulletSpawnPosition);
        
        // Draw an arrow to indicate firing direction
        if (Application.isPlaying && targetDamageable != null)
        {
            Gizmos.color = Color.green;
            Vector3 firingDirection = (targetDamageable.GetTransform().position + Vector3.up - bulletSpawnPosition).normalized;
            Gizmos.DrawRay(bulletSpawnPosition, firingDirection * 2f);
        }
        else 
        {
            // Default forward direction when no target
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(bulletSpawnPosition, transform.forward * 2f);
        }
    }
}
