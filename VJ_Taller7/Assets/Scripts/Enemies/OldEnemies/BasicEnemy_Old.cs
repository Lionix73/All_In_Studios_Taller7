using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.IO.Compression;

public class BasicEnemy_Old : EnemyBase_Old
{

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2.90f;
    [SerializeField] private float attackDamage = 10.0f;
    [SerializeField] private float attackRange = 1.0f;
    [SerializeField] private Transform attackPoint;
    private float lastAttackTime;

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
    private enum State { Chasing, Attacking }

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
            }
        }
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

        if (Vector3.Distance(transform.position, movePositionTransform.position) <= attackRange + attackPoint.localPosition.magnitude)
        {
            currentState = State.Attacking;
        }
    }

    public override void Attack()
    {
        if (!isStatic)
        {
            navMeshAgent.SetDestination(transform.position);
            RotateTowardsPlayer();
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            animator.SetTrigger("isAttacking");
            PerformAttack();
            lastAttackTime = Time.time;
        }

        if (!isStatic && !IsPlayerInSight())
        {
            currentState = State.Chasing;
        }
    }

    public void PerformAttack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, attackRange);
        foreach (var hitCollider in hitColliders)
        {
            PlayerController playerHealth = hitCollider.GetComponent<PlayerController>();
            if (playerHealth != null)
            {
                //playerHealth.TakeDamage(attackDamage);
                Debug.Log("Lo deje temblando: " + attackDamage + " de daño.");
            }
        }
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
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
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

    private void NavMeshInstatiator(){
        NavMeshHit hit;

        if(NavMesh.SamplePosition(transform.position, out hit, 500f, NavMesh.AllAreas)){
            transform.position = hit.position;
        }
        else{
            Debug.Log("No se pudo instanciar el NavMesh.");
        }
    }
}
