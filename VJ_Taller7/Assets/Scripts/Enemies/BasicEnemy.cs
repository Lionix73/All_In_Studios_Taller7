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
        navMeshAgent.SetDestination(transform.position);

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
        Debug.Log("Zambombazo!");

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
}
