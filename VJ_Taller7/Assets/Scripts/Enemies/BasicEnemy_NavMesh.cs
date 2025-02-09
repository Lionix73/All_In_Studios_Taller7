using UnityEngine;
using UnityEngine.AI;

public class BasicEnemy_NavMesh : MonoBehaviour
{
    [SerializeField] private Transform movePositionTransform;
    [SerializeField] private float attackRange = 2.0f;

    private NavMeshAgent navMeshAgent;
    private enum State { Chasing, Attacking }
    private State currentState;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        currentState = State.Chasing;
        movePositionTransform = GameObject.FindWithTag("Player").transform;
    }

    void Update()
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

    private void Chase()
    {
        navMeshAgent.SetDestination(movePositionTransform.position);

        if (Vector3.Distance(transform.position, movePositionTransform.position) <= attackRange)
        {
            currentState = State.Attacking;
        }
    }

    private void Attack()
    {
        navMeshAgent.SetDestination(transform.position);

        if(GetComponent<BasicEnemyShooter>() != null)
            GetComponent<BasicEnemyShooter>().PerformAttack();
        else if (GetComponent<BasicEnemy>() != null){
            GetComponent<BasicEnemy>().PerformAttack();
        }

        if (Vector3.Distance(transform.position, movePositionTransform.position) > attackRange)
        {
            currentState = State.Chasing;
        }
    }
}
