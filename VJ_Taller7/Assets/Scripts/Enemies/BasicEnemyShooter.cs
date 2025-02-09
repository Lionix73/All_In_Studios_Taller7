using UnityEngine;
using UnityEngine.AI;

public class BasicEnemyShooter : EnemyBase
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private float attackDamage = 10.0f;
    [SerializeField] private float attackRange = 6.0f;
    private float lastAttackTime;

    [Header("Bullet Settings")]
    [SerializeField] private float bulletSpeed = 10.0f;
    [SerializeField] private float bulletDamage = 10.0f;
    [SerializeField] private float bulletLifetime = 10.0f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;

    [Header("Movement Settings")]
    [SerializeField] private Transform movePositionTransform;
    [SerializeField] private bool isStatic = false;

    private NavMeshAgent navMeshAgent;
    private enum State { Chasing, Attacking }
    private State currentState;

    protected override void Initialize()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = speed;
        currentState = State.Chasing;
        movePositionTransform = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        if (isStatic)
        {
            Attack();
        }
        else
        {
            switch (currentState)
            {
                case State.Chasing:
                    Chase();
                    break;
                case State.Attacking:
                    Attack();
                    break;
            }
        }

        RotateTowardsPlayer();
    }

    protected override void Move()
    {
        if (navMeshAgent != null)
        {
            
        }
    }

    protected override void Chase()
    {
        navMeshAgent.SetDestination(movePositionTransform.position);

        if (Vector3.Distance(transform.position, movePositionTransform.position) <= attackRange)
        {
            currentState = State.Attacking;
            navMeshAgent.isStopped = true;
        }
    }

    public override void Attack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }

        if (Vector3.Distance(transform.position, movePositionTransform.position) > attackRange)
        {
            currentState = State.Chasing;
            navMeshAgent.isStopped = false; 
        }
    }

    public void PerformAttack()
    {
        Debug.Log("Disparo!");

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
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

    private void RotateTowardsPlayer()
    {
        Vector3 direction = (movePositionTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}
