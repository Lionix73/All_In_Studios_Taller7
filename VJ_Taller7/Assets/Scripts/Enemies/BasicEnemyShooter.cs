using UnityEngine;
using UnityEngine.AI;

public class BasicEnemyShooter : EnemyBase
{
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private float attackDamage = 10.0f;
    [SerializeField] private float bulletSpeed = 10.0f;
    [SerializeField] private float bulletDamage = 10.0f;
    [SerializeField] private float bulletLifetime = 2.0f;
    [SerializeField] private GameObject bulletPrefab;
    private float lastAttackTime;

    private NavMeshAgent navMeshAgent;

    protected override void Initialize()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = speed;
    }

    protected override void Move()
    {
        if (navMeshAgent != null)
        {
            
        }
    }

    public override void Attack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }

    public void PerformAttack()
    {
        Debug.Log("Zambombazo!");

        GameObject bullet = Instantiate(bulletPrefab, transform.position + transform.forward, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * bulletSpeed;
        }

        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            bulletScript.Damage = bulletDamage;
        }

        Destroy(bullet, bulletLifetime);
    }
}
