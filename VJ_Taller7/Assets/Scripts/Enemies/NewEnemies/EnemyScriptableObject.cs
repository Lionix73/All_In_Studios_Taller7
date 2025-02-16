using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[CreateAssetMenu(fileName = "Enemy ScriptableObject", menuName = "Enemies/Enemy ScriptableObject")]
public class EnemyScriptableObject : ScriptableObject
{
    // Enemy configuration
    public int health = 100;
    public float attackDelay = 1f;
    public int damage = 5;
    public float attackRadius = 1f;
    public bool isRanged = false;

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
}
