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

    [SerializeField] private SkillScriptableObject[] skills;
    public SkillScriptableObject[] Skills{
        get => skills;
        set => skills = value;
    }

    [SerializeField] private RagdollEnabler ragdollEnabler;
    public RagdollEnabler RagdollEnabler{
        get => ragdollEnabler;
        set => ragdollEnabler = value;
    }
    private Collider colliderEnemy;
    public Collider ColliderEnemy{
        get => colliderEnemy;
        set => colliderEnemy = value;
    }
    

    [Header("Enemy Health")]
    [SerializeField] private int health = 100;
    public int Health{
        get => health;
        set => health = value;
    }

    private bool isDead = false;
    public bool IsDead{
        get => isDead;
        set => isDead = value;
    }

    [SerializeField] private ProgressBar healthBar;
    public ProgressBar HealthBar{
        get => healthBar;
        set => healthBar = value;
    }

    private float maxHealth;


    [Header("Enemy Settings")]
    [SerializeField] private bool isStatic = false;
    public bool IsStatic{
        get => isStatic;
        set => isStatic = value;
    }

    [SerializeField] private float fadeOutDelay = 3f;

    private Coroutine lookCoroutine;


    [Header("Enemy Animator")]
    [SerializeField] private Animator animator;
    public Animator Animator{
        get => animator;
        set => animator = value;
    }

    public const string ATTACK_TRIGGER = "Attack";


    [Header("Enemy UI")]
    [SerializeField] GameObject floatingTextPrefab;

    private EnemySpawner enemySpawner;

    [Header("Enemy VFX")]
    [SerializeField] private ParticleSystem deathVFX;

    [Header("Game Manager")]
    //Evento que se llama para el game manager
    public int scoreOnKill;
    public event EventHandler<OnEnemyDeadEventArgs> OnEnemyDead;
    public class OnEnemyDeadEventArgs : EventArgs{
        public int score;
    }
    
    public delegate void DeathEvent(Enemy enemy);
    public DeathEvent OnDie; //Evento que se llama


    [Header("External Calls")]
    public Camera MainCamera { get; set; }
    public PlayerController Player { get; set; }
    public int Level { get; set; }

    private void Awake()
    {
        attackRadius.Player = Player;
        colliderEnemy = GetComponent<Collider>();
        AttackRadius.OnAttack += OnAttack;

        if(isStatic)
        enemySpawner = FindFirstObjectByType<EnemySpawner>();
    }

    private void Start()
    {
        maxHealth = Health;
    }

    private void Update()
    {
        if (isDead) return;

        for(int i = 0; i < skills.Length; i++){
            if(skills[i].CanUseSkill(this, Player, Level)){
                skills[i].UseSkill(this, Player);
            }
        }
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
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, lookRotation, time);
            transform.rotation = Quaternion.Euler(transform.rotation.x, targetRotation.eulerAngles.y, transform.rotation.z);
            
            time += Time.deltaTime * 2f;
            yield return null;
        }

        transform.rotation = lookRotation;
    }

    public override void OnDisable(){
        if(!isStatic){
            base.OnDisable();
            agent.enabled = false;
            OnDie = null;
        }
    }

    public void TakeDamage(int damage){
        if (isDead) return;

        Health -= damage;
        
        if (floatingTextPrefab != null)
        {
            ShowFloatingText(damage);
        }

        healthBar.SetProgress(Health / maxHealth, 3);

        if (Health <= 0){

            isDead = true;

            if(!isStatic){
                agent.enabled = false;
            }

            OnDie?.Invoke(this);

            if(ragdollEnabler != null){
                ragdollEnabler.EnableRagdoll();
            }
            
            StartCoroutine(FadeOut());
            //OnDied();
        }
    }

    private IEnumerator FadeOut(){
        if(deathVFX != null){
            deathVFX.Play();
        }

        yield return new WaitForSeconds(fadeOutDelay);

        if(ragdollEnabler != null){
            colliderEnemy.enabled = false;
            ragdollEnabler.DisableAllRigidbodies();
        }

        float time = 0;

        while(time < 1){
            transform.position += Vector3.down * Time.deltaTime;
            time += Time.deltaTime;
            yield return null;
        }

        healthBar.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public Transform GetTransform(){
        return transform;
    }

    /*
    private void OnDied(){
        float destroyDelay = UnityEngine.Random.value;
        gameObject.SetActive(false);

        if(!isStatic){
            healthBar.gameObject.SetActive(false);
        }

        if (isStatic && enemySpawner != null)
        {
            enemySpawner.RespawnEnemy(this);
        }

        OnEnemyDead?.Invoke(this, new OnEnemyDeadEventArgs{score = scoreOnKill});
    }
    */

    public void SetUpHealthBar(Canvas canvas, Camera mainCamera){
        healthBar.transform.SetParent(canvas.transform);
        healthBar.gameObject.SetActive(true);

        if (healthBar.TryGetComponent<FaceCamera>(out FaceCamera faceCamera))
        {
            faceCamera.Camera = mainCamera;
        }

        //Debug.Log("Heatlh: " + Health + " MaxHealth: " + maxHealth);
        healthBar.SetProgress(Health / maxHealth, 3);
    }
    private void ShowFloatingText(float damage)
    {
        var go = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity, transform);
        go.GetComponent<TextMesh>().text = damage.ToString();
    }
}
