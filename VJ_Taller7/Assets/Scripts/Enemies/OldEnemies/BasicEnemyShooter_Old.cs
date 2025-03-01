using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BasicEnemyShooter_Old : EnemyBase_Old
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 3f;
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

    [Header("NavMesh Settings")]
    private NavMeshAgent navMeshAgent;

    [Header("Animator Settings")]
    [SerializeField] private FloatDampener speedX;
    [SerializeField] private FloatDampener speedY;

    [Header("State Settings")]
    private State currentState;
    private enum State { Chasing, Attacking, MaintainingDistance, Retreating }

    private void Awake()
    {
        animator = GetComponent<Animator>();

        navMeshAgent = GetComponent<NavMeshAgent>();

        mainCollider = GetComponent<Collider>();
        colliders = GetComponentsInChildren<Collider>();
        rigidbodies = GetComponentsInChildren<Rigidbody>();

        spawnPoint = transform.position;
    }

    protected override void Initialize()
    {
        DisableRagdoll();

        health = maxHealth;
        navMeshAgent.speed = speed;
        currentState = State.Chasing;
        movePositionTransform = GameObject.FindWithTag("Player").transform;

        if(isStatic) navMeshAgent.enabled = false;
    }

    void Update()
    {
        HandleAnims();

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
                case State.MaintainingDistance:
                    MaintainDistance();
                    break;
                case State.Retreating:
                    Retreat();
                    break;
            }
        }

        RotateTowardsPlayer();
    }

    protected override void HandleAnims()
    {
        if(navMeshAgent.hasPath){
            Vector3 dir = (navMeshAgent.steeringTarget - transform.position).normalized;
            Vector3 animDir = transform.InverseTransformDirection(dir);

            speedX.Update();
            speedY.Update();

            speedX.TargetValue = animDir.x;
            speedY.TargetValue = animDir.z;

            animator.SetFloat("Horizontal", speedX.CurrentValue);
            animator.SetFloat("Vertical", speedY.CurrentValue);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), 180 * Time.deltaTime);

            if(Vector3.Distance(transform.position, navMeshAgent.destination) <= navMeshAgent.radius){
                //navMeshAgent.ResetPath();
            }
        }
        else{
            animator.SetFloat("Horizontal", 0, 0.25f, Time.deltaTime);
            animator.SetFloat("Vertical", 0, 0.25f, Time.deltaTime);
        }    
    }

    protected override void Move()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(movePositionTransform.position);
        }
        else
        {
            navMeshAgent.isStopped = true;
        }
    }

    protected override void Chase()
    {
        Move();

        if (Vector3.Distance(transform.position, movePositionTransform.position) <= attackRange)
        {
            currentState = State.Attacking;
            navMeshAgent.isStopped = true;
        }
        else if (Vector3.Distance(transform.position, movePositionTransform.position) <= attackRange * 2)
        {
            currentState = State.MaintainingDistance;
        }
    }

    public override void Attack()
    {
        if (Time.time >= lastAttackTime + attackCooldown && IsPlayerInSight())
        {
            animator.SetTrigger("isAttacking");
            //PerformAttack();
            lastAttackTime = Time.time;
        }

        if (!isStatic)
        {
            if (Vector3.Distance(transform.position, movePositionTransform.position) > attackRange)
            {
                currentState = State.Chasing;
                navMeshAgent.isStopped = false;
            }
            else if (Vector3.Distance(transform.position, movePositionTransform.position) < attackRange / 2)
            {
                currentState = State.Retreating;
            }
        }
    }

    private void PerformAttack()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * bulletSpeed;
        }

        EnemyBullet_Old bulletScript = bullet.GetComponent<EnemyBullet_Old>();
        if (bulletScript != null)
        {
            bulletScript.Damage = bulletDamage;
        }

        Destroy(bullet, bulletLifetime);
    }

    private void MaintainDistance()
    {
        if (Vector3.Distance(transform.position, movePositionTransform.position) > attackRange)
        {
            currentState = State.Chasing;
        }
        else if (Vector3.Distance(transform.position, movePositionTransform.position) < attackRange / 2)
        {
            currentState = State.Retreating;
        }
        else
        {
            navMeshAgent.isStopped = true;
            Attack();
        }
    }

    private void Retreat()
    {
        Vector3 direction = (transform.position - movePositionTransform.position).normalized;
        Vector3 retreatPosition = transform.position + direction * attackRange;

        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(retreatPosition);

        if (Vector3.Distance(transform.position, movePositionTransform.position) >= attackRange)
        {
            currentState = State.MaintainingDistance;
        }
    }

    private void RotateTowardsPlayer()
    {
        Vector3 direction = (movePositionTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private bool IsPlayerInSight()
    {
        if (Vector3.Distance(transform.position, movePositionTransform.position) <= attackRange)
        {
            RaycastHit hit;
            Vector3 direction = (movePositionTransform.position - transform.position).normalized;
            if (Physics.Raycast(transform.position, direction, out hit, attackRange))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    protected override void TriggerHitAnimation()
    {
        base.TriggerHitAnimation();
    }

    protected override void Die()
    {
        if (respawnOnDeath)
        {
            StartCoroutine(Respawn());
        }
        else
        {
            Destroy(gameObject);
        }
    }   

    private IEnumerator Respawn()
    {
        EnableRagdoll();
        yield return new WaitForSeconds(respawnTime);
        health = maxHealth;
        healthBar.UpdateHealthBar(health, maxHealth);
        transform.position = spawnPoint;
        currentState = State.Chasing;
        DisableRagdoll();
    }

    private void OnDrawGizmos()
    {
        if(navMeshAgent == null) return;

        if(navMeshAgent.hasPath){
            for(int i = 0; i < navMeshAgent.path.corners.Length - 1; i++){
            Debug.DrawLine(navMeshAgent.path.corners[i], navMeshAgent.path.corners[i + 1], Color.red);
            }
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }  
}
