using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using Unity.VisualScripting;

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

    [SerializeField] private Collider[] colliderEnemy;
    public Collider[] ColliderEnemy{
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
    private CarnivoroTemporal[] carnivoros;

    [Header("Enemy Animator")]
    [SerializeField] private Animator animator;
    public Animator Animator{
        get => animator;
        set => animator = value;
    }

    public const string ATTACK_TRIGGER = "Attack";
    public const string STOPATTACK_TRIGGER = "StopAttack";
    public const string SHOOT_TRIGGER = "Shoot";
    public const string SKILL_TRIGGER = "UsingSkill";
    public const string IsHit = "IsHit";


    [Header("Enemy UI")]
    public GameObject floatingTextPrefab;
    public GameObject floatingTextCriticPrefab;

    private EnemySpawner enemySpawner;
    private ThisObjectSounds soundManager;
    private OnomatopoeiaController onomatopeia;

    [Header("Enemy VFX")]
    [SerializeField] private ParticleSystem deathVFX;
    [SerializeField] private SkinnedMeshRenderer[] skinnedMeshRenderers;
    [SerializeField] private float blinkIntensity;
    [SerializeField] private float blinkDuration;

    private float blinkTimer;


    //Evento que se llama para el game manager --Antigua referencia, esta con el balancing in this del manager (ignorar perop dejar quieto)
    public event EventHandler<OnEnemyDeadEventArgs> OnEnemyDead;
    public class OnEnemyDeadEventArgs : EventArgs{
        public int score;
    }
    
    public delegate void DeathEvent(Enemy enemy);
    public DeathEvent OnDie; //Evento que se llama

    [Header("Game Manager")]
    public int scoreOnKill;


    [Header("External Calls")]
    public Camera MainCamera { get; set; }
    public PlayerController Player { get; set; }

    public int Level { get; set; }

    private RoundManager roundManager;
    
    private void OnEnable()
    {
        if (!isStatic)
        {
            agent.enabled = true;
            isDead = false;

            foreach (Collider collider in colliderEnemy)
            {
                collider.enabled = true;
            }
        }
    }
    
    public override void OnDisable()
    {
        if (!isStatic)
        {
            base.OnDisable();

            agent.enabled = false;
            OnDie = null;
        }
    }

    private void Awake()
    {
        attackRadius.Player = Player;
        soundManager = GetComponent<ThisObjectSounds>();
        onomatopeia = GetComponentInChildren<OnomatopoeiaController>();
        AttackRadius.OnAttack += OnAttack;

        roundManager = GameManager.Instance.roundManager;

        if (skinnedMeshRenderers == null)
        {
            skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        if (isStatic)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
    }

    private void Start()
    {
        maxHealth = Health;
        carnivoros = FindObjectsByType<CarnivoroTemporal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    }

    private void Update()
    {
        if (isDead) return;

        BlinkEffect();

        for (int i = 0; i < skills.Length; i++)
        {
            if (skills[i].CanUseSkill(this, Player, Level))
            {
                skills[i].UseSkill(this, Player);
            }
        }
    }

    public void PlaySpawnSound()
    {
        soundManager.PlaySound("Spawn");
    }

    private void OnAttack(IDamageable target)
    {
        if(lookCoroutine != null){
            StopCoroutine(lookCoroutine);
        }

        lookCoroutine = StartCoroutine(LookAt(target.GetTransform()));
    }

    private IEnumerator LookAt(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0; // Ignore the Y-axis

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        float time = 0f;

        while (time < 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, agent.angularSpeed * time);
            time += Time.deltaTime * 2f;
            yield return null;
        }

        transform.rotation = lookRotation;
    }

    public void TakeDamage(int damage){
        if (isDead) return;

        Health -= damage;
        blinkTimer = blinkDuration;

        animator.SetTrigger(IsHit);
        movement.State = EnemyState.Chase;
        healthBar.SetProgress(Health / maxHealth, 3);

        //Debug.Log("Enemy Health: " + Health);

        if (Health <= 0)
        {
            onomatopeia.TryShowOnomatopoeia();
            soundManager.StopAllSounds();
            soundManager.PlaySound("Die");

            #region CarnivoroTemporal Skill

            foreach(CarnivoroTemporal c in carnivoros)
            {
                if(c.CarnivoroActive)
                {
                    c.HealthForKill();
                }
            }
            #endregion

            /*
            // Use thread-safe invocation for the event
            DeathEvent handler = OnDie;
            if (handler != null) {
                // This ensures all subscribers are notified atomically
                handler(this);
            }
            */

            roundManager.EnemyDied(this);
            roundManager.ChangeScore(this);
            isDead = true;

            if (!isStatic)
            {
                agent.enabled = false;

                foreach (Collider collider in colliderEnemy)
                {
                    collider.enabled = false;
                }
            }

            if(ragdollEnabler != null){
                ragdollEnabler.EnableRagdoll();
            }
            
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeOut(){
        if(deathVFX != null){
            deathVFX.Play();
        }

        yield return new WaitForSeconds(fadeOutDelay);

        if(ragdollEnabler != null){

            foreach (Collider collider in colliderEnemy)
            {
                collider.enabled = false;
            }

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

    public void ShowFloatingText(float damage, GameObject floatingTextPrefab)
    {
        var go = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity, transform);
        go.GetComponentInChildren<TextMeshPro>().text = damage.ToString();
    }

    public void BlinkEffect(){
        if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0) return;

        blinkTimer -= Time.deltaTime;

        float lerp = Mathf.Clamp01(blinkTimer / blinkDuration);
        float intensity = (lerp * blinkIntensity) + 1.0f;
        
        foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
        {
            skinnedMeshRenderer.material.color = Color.white * intensity;
        }
    }
}
