using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Enemy ScriptableObject", menuName = "Enemies/Enemy ScriptableObject")]
public class EnemyScriptableObject : ScriptableObject
{
    public Enemy prefab;
    public EnemyMulti prefabMulti;
    public AttackScriptableObject attackConfiguration;
    public SkillScriptableObject[] skills;

    // Enemy configuration
    public int health = 100;

    //Movement Stats
    public EnemyState defaultState;
    public float idleLocationRadius = 4f;
    public float idleMoveSpeedMultiplier = 0.5f;
    [Range(2, 10)] public int numberOfWaypoints = 4;
    public float lineOfSightRadius = 6f;
    public float fov = 90f;

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

    public EnemyScriptableObject ScaleUpLevel(ScalingScriptableObject scaling, int level){
        EnemyScriptableObject scaledEnemy = CreateInstance<EnemyScriptableObject>();
        scaledEnemy.name = name;
        scaledEnemy.prefab = prefab;
        scaledEnemy.prefabMulti = prefabMulti;

        scaledEnemy.attackConfiguration = attackConfiguration.ScaleUpLevel(scaling, level);

        scaledEnemy.skills = new SkillScriptableObject[skills.Length];
        for(int i = 0; i < skills.Length; i++){
            scaledEnemy.skills[i] = skills[i].scaleUpForLevel(scaling, level);
        }

        scaledEnemy.health = Mathf.FloorToInt(health * scaling.healthCurve.Evaluate(level));

        scaledEnemy.defaultState = defaultState;
        scaledEnemy.idleLocationRadius = idleLocationRadius;
        scaledEnemy.idleMoveSpeedMultiplier = idleMoveSpeedMultiplier;
        scaledEnemy.numberOfWaypoints = numberOfWaypoints;
        scaledEnemy.lineOfSightRadius = lineOfSightRadius;
        scaledEnemy.fov = fov;

        scaledEnemy.aIUpdateInterval = aIUpdateInterval;
        scaledEnemy.acceleration = acceleration;
        scaledEnemy.angularSpeed = angularSpeed;

        scaledEnemy.areaMask = areaMask;
        scaledEnemy.avoidancePriority = avoidancePriority;

        scaledEnemy.baseOffset = baseOffset;
        scaledEnemy.height = height;
        scaledEnemy.obstacleAvoidanceType = obstacleAvoidanceType;
        scaledEnemy.radius = radius;
        scaledEnemy.speed = speed * scaling.speedCurve.Evaluate(level);
        scaledEnemy.stoppingDistance = stoppingDistance;

        return scaledEnemy;
    }

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
        enemy.Movement.DefaultState = defaultState;
        enemy.Movement.IdleMoveSpeedMultiplier = idleMoveSpeedMultiplier;
        enemy.Movement.IdleLocationRadius = idleLocationRadius;
        enemy.Movement.Waypoints = new Vector3[numberOfWaypoints];
        enemy.Movement.LineOfSightChecker.Fov = fov;
        enemy.Movement.LineOfSightChecker.SphereCollider.radius = lineOfSightRadius;
        enemy.Movement.LineOfSightChecker.LineOfSightMask = attackConfiguration.lineOfSightLayer;

        enemy.Health = health;

        attackConfiguration.SetUpEnemey(enemy);
    }
    public void SetUpEnemyMulti(EnemyMulti enemy)
    {
        SetUpEnemyAgentMulti(enemy);

        
        enemy.Movement.DefaultState = defaultState;
        enemy.Movement.IdleMoveSpeedMultiplier = idleMoveSpeedMultiplier;
        enemy.Movement.IdleLocationRadius = idleLocationRadius;
        enemy.Movement.Waypoints = new Vector3[numberOfWaypoints];
        enemy.Movement.LineOfSightChecker.Fov = fov;
        enemy.Movement.LineOfSightChecker.SphereCollider.radius = lineOfSightRadius;
        enemy.Movement.LineOfSightChecker.LineOfSightMask = attackConfiguration.lineOfSightLayer;

        enemy.Health = health;
        enemy.maxHealth = health;
        attackConfiguration.SetUpEnemyMulti(enemy);
    }
    public void SetUpEnemyAgentMulti(EnemyMulti enemy)
    {
        enemy.Movement.UpdateRate = aIUpdateInterval;

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
    }

}
