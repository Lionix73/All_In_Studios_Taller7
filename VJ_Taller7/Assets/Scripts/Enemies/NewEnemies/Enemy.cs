using UnityEngine;
using UnityEngine.AI;

public class Enemy : PoolableObject
{
    [SerializeField] private EnemyMovement movement;
    public EnemyMovement Movement{
        get => movement;
        set => movement = value;
    }
    
    [SerializeField] private NavMeshAgent agent;
    public NavMeshAgent Agent{
        get => agent;
        set => agent = value;
    }

    [SerializeField] private EnemyScriptableObject enemyConfiguration;
    
    [SerializeField] private int health = 100;
    public int Health{
        get => health;
        set => health = value;
    }

    public virtual void OnEnable(){
        SetUpAgentFromConfiguration();
    }

    public override void OnDisable(){
        base.OnDisable();

        Agent.enabled = false;
    }

    public virtual void SetUpAgentFromConfiguration(){
        Agent.acceleration = enemyConfiguration.acceleration;
        Agent.angularSpeed = enemyConfiguration.angularSpeed;
        Agent.areaMask = enemyConfiguration.areaMask;
        Agent.avoidancePriority = enemyConfiguration.avoidancePriority;
        Agent.baseOffset = enemyConfiguration.baseOffset;
        Agent.height = enemyConfiguration.height;
        Agent.obstacleAvoidanceType = enemyConfiguration.obstacleAvoidanceType;
        Agent.radius = enemyConfiguration.radius;
        Agent.speed = enemyConfiguration.speed;
        Agent.stoppingDistance = enemyConfiguration.stoppingDistance;

        Movement.UpdateRate = enemyConfiguration.aIUpdateInterval;

        Health = enemyConfiguration.health;
    } 
}
