using System;
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
    
    [Header("Enemy Health")]
    [SerializeField] private int health = 100;
    public int Health{
        get => health;
        set => health = value;
    }

    [SerializeField] private ProgressBar healthBar;

    public Camera MainCamera { get; set; }

    private float maxHealth;

    [Header("Enemy Settings")]
    [SerializeField] private bool isStatic = false;
    public bool IsStatic{
        get => isStatic;
        set => isStatic = value;
    }

    private Coroutine lookCoroutine;

    [Header("Enemy Animator")]
    [SerializeField] private Animator animator;
    private const string ATTACK_TRIGGER = "Attack";

    [Header("Enemy UI")]
    [SerializeField] GameObject floatingTextPrefab;

    private EnemySpawner enemySpawner;

    [Header("Game Manager")]
    //Evento que se llama para el game manager
    [SerializeField] private int scoreOnKill;
    public event EventHandler<OnEnemyDeadEventArgs> OnEnemyDead;
    public class OnEnemyDeadEventArgs : EventArgs{
        public int score;
    }

    private void Awake()
    {
        AttackRadius.OnAttack += OnAttack;
        maxHealth = health;

        if(isStatic)
        enemySpawner = FindFirstObjectByType<EnemySpawner>();
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

    public override void OnDisable(){
        if(!isStatic){
            base.OnDisable();
            agent.enabled = false;
        }
    }

    public void TakeDamage(int damage){
        health -= damage;
        
        if (floatingTextPrefab != null)
        {
            ShowFloatingText(damage);
        }

        healthBar.SetProgress(health / maxHealth, 3);

        if (health <= 0){

            if(!isStatic){
                agent.ResetPath();
                agent.enabled = false;
            }

            OnDied();
        }
    }

    public Transform GetTransform(){
        return transform;
    }

    private void OnDied(){
        float destroyDelay = UnityEngine.Random.value;
        gameObject.SetActive(false);

        if(!isStatic){
            Destroy(healthBar.gameObject, destroyDelay);
        }

        if (isStatic && enemySpawner != null)
        {
            enemySpawner.RespawnEnemy(this);
        }

        OnEnemyDead?.Invoke(this, new OnEnemyDeadEventArgs{score = scoreOnKill});
    }

    public void SetUpHealthBar(Canvas canvas, Camera mainCamera){
        healthBar.transform.SetParent(canvas.transform);

        if (healthBar.TryGetComponent<FaceCamera>(out FaceCamera faceCamera))
        {
            faceCamera.Camera = mainCamera;
        }
    }
    private void ShowFloatingText(float damage)
    {
        var go = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity, transform);
        go.GetComponent<TextMesh>().text = damage.ToString();
    }
}
