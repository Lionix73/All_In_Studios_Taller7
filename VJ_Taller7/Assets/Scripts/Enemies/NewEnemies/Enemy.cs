using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : PoolableObject, IDamageable
{
    [Header("Enemy Components")]
    [SerializeField] private AttackRadius attackRadius;
    public AttackRadius AttackRadius{
        get => attackRadius;
        set => attackRadius = value;
    }

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
    
    [Header("Enemy Health")]
    [SerializeField] private int health = 100;
    public int Health{
        get => health;
        set => health = value;
    }

    private Coroutine lookCoroutine;

    [Header("Enemy Animator")]
    [SerializeField] private Animator animator;
    private const string ATTACK_TRIGGER = "Attack";


    private void Awake()
    {
        AttackRadius.OnAttack += OnAttack;
    }

    private void OnAttack(IDamageable target)
    {
        animator.SetTrigger(ATTACK_TRIGGER);

        if(lookCoroutine != null){
            StopCoroutine(lookCoroutine);
        }

        lookCoroutine = StartCoroutine(LookAt(target.GetTransform()));
    }

    private IEnumerator LookAt(Transform target)
    {
        Quaternion lookRotation = Quaternion.LookRotation(target.position - transform.position);
        float time = 0f;

        while(time < 1f){
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, time);
            time += Time.deltaTime * 2f;
            yield return null;
        }

        transform.rotation = lookRotation;
    }

    public virtual void OnEnable(){
        SetUpAgentFromConfiguration();
    }

    public override void OnDisable(){
        base.OnDisable();

        agent.enabled = false;
    }

    public virtual void SetUpAgentFromConfiguration(){
        agent.acceleration = enemyConfiguration.acceleration;
        agent.angularSpeed = enemyConfiguration.angularSpeed;
        agent.areaMask = enemyConfiguration.areaMask;
        agent.avoidancePriority = enemyConfiguration.avoidancePriority;
        agent.baseOffset = enemyConfiguration.baseOffset;
        agent.height = enemyConfiguration.height;
        agent.obstacleAvoidanceType = enemyConfiguration.obstacleAvoidanceType;
        agent.radius = enemyConfiguration.radius;
        agent.speed = enemyConfiguration.speed;
        agent.stoppingDistance = enemyConfiguration.stoppingDistance;

        movement.UpdateRate = enemyConfiguration.aIUpdateInterval;

        health = enemyConfiguration.health;
        
        (attackRadius.sphereCollider == null ? attackRadius.GetComponent<SphereCollider>() : attackRadius.sphereCollider).radius = enemyConfiguration.attackRadius;
        attackRadius.AttackDelay = enemyConfiguration.attackDelay;
        attackRadius.Damage = enemyConfiguration.damage;
    }

    public void TakeDamage(int damage){
        health -= damage;

        if (health <= 0){
            gameObject.SetActive(false);
        }
    }

    public Transform GetTransform(){
        return transform;
    }
}
