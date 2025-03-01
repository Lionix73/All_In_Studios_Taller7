using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Enemy ScriptableObject", menuName = "Enemies/Enemy ScriptableObject")]
public class EnemyScriptableObject : ScriptableObject
{
    public Enemy prefab;
    public AttackScriptableObject attackConfiguration;

    // Enemy configuration
    public int health = 100;

    // NavMeshAgent configuration
    public float aIUpdateInterval = 0.1f;

    // NavMeshAgent properties
    public float acceleration = 8f;
    public float angularSpeed = 120f;
    public int areaMask = -1;
    public int avoidancePriority = 50;
    public float baseOffset = 0f;
    public float height = 2f;
    public ObstacleAvoidanceType obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
    public float radius = 0.5f;
    public float speed = 3f;
    public float stoppingDistance = 0.5f;

    public void SetUpEnemy(Enemy enemy){
        enemy.Agent.acceleration = acceleration;
        enemy.Agent.angularSpeed = angularSpeed;
        enemy.Agent.areaMask = areaMask;
        enemy.Agent.avoidancePriority = avoidancePriority;
        enemy.Agent.baseOffset = baseOffset;
        enemy.Agent.height = height;
        enemy.Agent.obstacleAvoidanceType = obstacleAvoidanceType;
        enemy.Agent.radius = radius;
        enemy.Agent.speed = speed;
        enemy.Agent.stoppingDistance = stoppingDistance;

        enemy.Movement.UpdateRate = aIUpdateInterval;

        enemy.Health = health;

        attackConfiguration.SetUpEnemey(enemy);
    }
}
