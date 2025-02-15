using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[CreateAssetMenu(fileName = "Enemy ScriptableObject", menuName = "Enemies/Enemy ScriptableObject")]
public class EnemyScriptableObject : ScriptableObject
{
    public int health = 100;

    public float aIUpdateInterval = 0.1f;

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
