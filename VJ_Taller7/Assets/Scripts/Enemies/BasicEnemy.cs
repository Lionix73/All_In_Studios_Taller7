using UnityEngine;
using UnityEngine.AI;

public class BasicEnemy : EnemyBase
{
    private NavMeshAgent navMeshAgent;

    protected override void Initialize()
    {
        //navMeshAgent = GetComponent<NavMeshAgent>();
        //navMeshAgent.speed = speed;
    }

    protected override void Move()
    {
        if (navMeshAgent != null)
        {
            
        }
    }

    public override void Attack()
    {
        // Futuramente...
    }
}
