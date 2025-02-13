using System;
using UnityEngine;
using UnityEngine.AI;

public class BasicEnemy : EnemyBase
{

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private float attackDamage = 10.0f;
    [SerializeField] private float attackRange = 1.0f;
    private float lastAttackTime;

    [Header("Movement Settings")]
    [SerializeField] private Transform movePositionTransform;
    [SerializeField] private bool isStatic = false;

    [Header("NavMesh Settings")]
    private NavMeshAgent navMeshAgent;

    [Header("Animator Settings")]
    [SerializeField] private FloatDampener speedX;
    [SerializeField] private FloatDampener speedY;
    private Animator animator;

    [Header("State Settings")]
    private State currentState;
    private enum State { Chasing, Attacking }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    protected override void Initialize()
    {
        health = maxHealth;
        navMeshAgent.speed = speed;
        currentState = State.Chasing;
        movePositionTransform = GameObject.FindWithTag("Player").transform;
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
            
        }
    }

    protected override void Chase()
    {
        navMeshAgent.SetDestination(movePositionTransform.position);

        if (Vector3.Distance(transform.position, movePositionTransform.position) <= attackRange)
        {
            currentState = State.Attacking;
        }
    }

    public override void Attack()
    {   
        if(!isStatic){
            navMeshAgent.SetDestination(transform.position);
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }

        if (Vector3.Distance(transform.position, movePositionTransform.position) > attackRange)
        {
            currentState = State.Chasing;
        }
    }

    public void PerformAttack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
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

    private void OnDrawGizmos()
    {
        if(navMeshAgent == null) return;

        if(navMeshAgent.hasPath){
            for(int i = 0; i < navMeshAgent.path.corners.Length - 1; i++){
            Debug.DrawLine(navMeshAgent.path.corners[i], navMeshAgent.path.corners[i + 1], Color.red);
            }
        }
    }
}
