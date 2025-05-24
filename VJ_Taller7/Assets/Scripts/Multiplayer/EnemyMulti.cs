using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using Unity.Netcode;


public class EnemyMulti : PoolableObjectMulti, IDamageableMulti
{
    [Header("Enemy Components")]
    [SerializeField] private MultiAttackRadius attackRadius;
    public MultiAttackRadius AttackRadius
    {
        get => attackRadius;
        set => attackRadius = value;
    }
    [SerializeField] private MultiLineOfSightChecker lineOfSightChecker;
    public MultiLineOfSightChecker LineOfSightChecker
    {
        get => lineOfSightChecker;
        set => lineOfSightChecker = value;
    }

    [SerializeField] private MultiEnemyMovement movement;
    public MultiEnemyMovement Movement
    {
        get => movement;
        set => movement = value;
    }

    [SerializeField] private NavMeshAgent agent;
    public NavMeshAgent Agent
    {
        get => agent;
        set => agent = value;
    }

    [SerializeField] private SkillScriptableObject[] skills;
    public SkillScriptableObject[] Skills
    {
        get => skills;
        set => skills = value;
    }

    [SerializeField] private RagdollEnabler ragdollEnabler;
    public RagdollEnabler RagdollEnabler
    {
        get => ragdollEnabler;
        set => ragdollEnabler = value;
    }
    [SerializeField] private Collider[] colliderEnemy;
    public Collider [] ColliderEnemy
    {
        get => colliderEnemy;
        set => colliderEnemy = value;
    }

    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>();


    [Header("Enemy Health")]
    public float maxHealth;
    [SerializeField] private ProgressBar healthBar;

    private NetworkVariable<int> networkHealth = new NetworkVariable<int>();
    public int Health
    {
        get => networkHealth.Value;
        set { if (IsServer) networkHealth.Value = value; }
    }
    private NetworkVariable<bool> networkIsDead = new NetworkVariable<bool>(false);
    public bool IsDead
    {
        get => networkIsDead.Value;
        set { if (IsServer) networkIsDead.Value = value; }
    }

  
    public ProgressBar HealthBar
    {
        get => healthBar;
        set => healthBar = value;
    }



    [Header("Enemy Settings")]
    [SerializeField] private bool isStatic = false;
    public bool IsStatic
    {
        get => isStatic;
        set => isStatic = value;
    }

    [SerializeField] private float fadeOutDelay = 3f;

    private Coroutine lookCoroutine;


    [Header("Enemy Animator")]
    [SerializeField] private Animator animator;
    public Animator Animator
    {
        get => animator;
        set => animator = value;
    }

    public const string ATTACK_TRIGGER = "Attack";
    public const string SHOOT_TRIGGER = "Shoot";
    public const string SKILL_TRIGGER = "UsingSkill";
    public const string IsHit = "IsHit";


    [Header("Enemy UI")]
    public GameObject floatingTextPrefab;
    public GameObject floatingTextCriticPrefab;

    private EnemySpawner enemySpawner;

    [Header("Enemy VFX")]
    [SerializeField] private ParticleSystem deathVFX;
    [SerializeField] private SkinnedMeshRenderer[] skinnedMeshRenderers;
    [SerializeField] private float blinkIntensity;
    [SerializeField] private float blinkDuration;

    private float blinkTimer;

    private MultiRoundManager multiRoundManager;


    //Evento que se llama para el game manager --Antigua referencia, esta con el balancing in this del manager (ignorar perop dejar quieto)
    public event EventHandler<OnEnemyDeadEventArgs> OnEnemyDead;
    public class OnEnemyDeadEventArgs : EventArgs
    {
        public int score;
    }

    public delegate void AttackerEvent(EnemyMulti enemy,ulong attackerId);
    public AttackerEvent GetAttackerId;

    public delegate void DeathEvent(EnemyMulti enemy);
    public DeathEvent OnDie; //Evento que se llama
    [Header("Game Manager")]
    public int scoreOnKill;
    public ulong lastAttackerId;

    [Header("External Calls")]
    public Camera MainCamera { get; set; }
    public PlayerControllerMulti Player { get; set; }
    public int Level { get; set; }

    public void RespawmEnemy()
    {
        RagdollEnabler.EnableAnimator();
        RagdollEnabler.DisableAllRigidbodies();
        if (!isStatic)
        {

            IsDead = false;

            foreach (Collider collider in colliderEnemy)
            {
                collider.enabled = true;
            }
        }
        HealthBarProgressRpc((int)maxHealth, maxHealth);

        RespawmEnemyRpc();
    }

    [Rpc(SendTo.Everyone)]
    public void RespawmEnemyRpc()
    {
        if(IsServer) return;

        RagdollEnabler.EnableAnimator();
        RagdollEnabler.DisableAllRigidbodies();
        if (!isStatic)
        {
              //agent.enabled = false;
            //agent.baseOffset = baseOffset;
            
            IsDead = false;

            foreach (Collider collider in colliderEnemy)
            {
                collider.enabled = true;
            }
        }
        HealthBarProgressRpc((int)maxHealth, maxHealth);
        gameObject.SetActive(true);
        Debug.Log(agent.isActiveAndEnabled);
    }
    private void Awake()
    {
      /*  if (!IsServer) return;

        attackRadius.Player = Player;
        colliderEnemy = GetComponent<Collider>();
        AttackRadius.OnAttack += OnAttack;

        if (skinnedMeshRenderers == null)
        {
            skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        //if (isStatic)
          //  enemySpawner = FindFirstObjectByType<EnemySpawner>();*/
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        
        multiRoundManager = GameObject.FindFirstObjectByType<MultiRoundManager>();
        attackRadius.Player = Player;
        LineOfSightChecker.OnGainSight += GetPlayer;
        LineOfSightChecker.OnLoseSight += LostPlayer;
        AttackRadius.OnAttack += OnAttack;

        if (skinnedMeshRenderers == null)
        {
            skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        //Health = (int)maxHealth;
    }

    private void LostPlayer(PlayerControllerMulti player)
    {
        Player = null;
    }

    private void Update()
    {
        if (IsDead) return;
        BlinkEffect();
        if (!IsServer) return;


       if (Player == null) return;

        for (int i = 0; i < skills.Length; i++)
        {
            if (skills[i].MultiCanUseSkill(this, Player, Level))
            {
                skills[i].MultiUseSkill(this, Player);
            }
        }
    }

    private void OnAttack(IDamageableMulti target)
    {
        animator.SetTrigger(ATTACK_TRIGGER);

        if (lookCoroutine != null)
        {
            StopCoroutine(lookCoroutine);
        }

        lookCoroutine = StartCoroutine(LookAt(target.GetTransform()));
    }

    private IEnumerator LookAt(Transform target)
    {
        Quaternion lookRotation = Quaternion.LookRotation(target.position - transform.position);
        float time = 0f;

        while (time < 1f)
        {

            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, time);
            time += Time.deltaTime * 2f;
            yield return null;

        }

        transform.rotation = lookRotation;
        SyncRotationClientRpc(transform.rotation);

    }
    [Rpc(SendTo.Everyone)]
    private void SyncRotationClientRpc(Quaternion newRotation)
    {
        if (!IsServer) // Solo clientes actualizan su posición
        {
            transform.rotation = newRotation;
        }
    }

    public override void OnDisable()
    {
        if (!IsServer) return;
        if (!isStatic)
        {
            base.OnDisable();
            agent.enabled = false;
            OnDie = null;
        }
    }
    [Rpc(SendTo.Server)]
    public void TakeDamageRpc(int damage, ulong clientId)
    {
        if(IsDead) return;

        lastAttackerId = clientId;
        //networkHealth.Value -= damage;
        int actualHealth = Health - damage;
        Health = actualHealth;

        animator.SetTrigger(IsHit);
        movement.State = EnemyState.Chase;
        healthBar.SetProgress(Health / maxHealth, 3);

        //Debug.Log("Enemy Health: " + Health);
        Debug.Log("Cliente #"+ lastAttackerId);
        HealthBarProgressRpc(actualHealth, maxHealth);


        if (Health <= 0)
        {
            //networkIsDead.Value = true;


            /* #region CarnivoroTemporal Skill
             // Check if carnivoro skill is active
             CarnivoroTemporal skill = FindAnyObjectByType(typeof(CarnivoroTemporal)).GetComponent<CarnivoroTemporal>();

             if (skill != null && skill.Carnivoro)
             {
                 skill.HealthForKill();
             }
             #endregion
            */

            // Use thread-safe invocation for the event
            /*DeathEvent handler = OnDie;
            if (handler != null)
            {
                Debug.Log("HandlerDeath");
                // This ensures all subscribers are notified atomically
                handler(this);
            }*/
            multiRoundManager.EnemyDied(this,lastAttackerId);
            IsDead = true;


            
            //GetAttackerId?.Invoke(this, lastAttackerId);
            //Debug.Log("Cliente #" + lastAttackerId+" Mato");
            HandleDeathOnServerRpc();
            //OnDied();
        }
    }

    [Rpc(SendTo.Server)]
    private void HandleDeathOnServerRpc()
    {
        agent.enabled = false;
        Debug.Log("Enemigo Muerto");
        // Replicar a todos los clientes
        DiedAnimationRpc();

    }

    public void TakeDamage(int damage, ulong clientId)
    {
        if (IsDead) return;

        TakeDamageRpc(damage, clientId);
    }

    [Rpc(SendTo.Everyone)]
    public void DiedAnimationRpc()
    {
        agent.enabled = false;

        if (!isStatic)
        {
            //agent.enabled = false;
            foreach (Collider collider in colliderEnemy)
            {
                collider.enabled = false;
            }
        }
        if (ragdollEnabler != null)
        {
            ragdollEnabler.EnableRagdoll();
        }

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        if (deathVFX != null)
        {
            deathVFX.Play();
        }

        yield return new WaitForSeconds(fadeOutDelay);

        if (ragdollEnabler != null)
        {

            foreach (Collider collider in colliderEnemy)
            {
                collider.enabled = false;
            }

            ragdollEnabler.DisableAllRigidbodies();
        }

        float time = 0;

        while (time < 1)
        {
            transform.position += Vector3.down * Time.deltaTime;
            time += Time.deltaTime;
            yield return null;
        }

        //healthBar.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public Transform GetTransform()
    {
        networkPosition.Value = transform.position;
        networkRotation.Value = transform.rotation;
        return transform;
    }

    public void SetUpHealthBar(Canvas canvas, Camera mainCamera)
    {
        healthBar.transform.SetParent(canvas.transform);
        healthBar.gameObject.SetActive(true);

        if (healthBar.TryGetComponent<FaceCamera>(out FaceCamera faceCamera))
        {
            faceCamera.Camera = mainCamera;
        }

        //Debug.Log("Heatlh: " + Health + " MaxHealth: " + maxHealth);
        //HealthBarProgressRpc();
    }
    [Rpc(SendTo.Everyone)]
    public void HealthBarProgressRpc(int currentHealth, float maxHealth)
    {
        blinkTimer = blinkDuration;
        healthBar.SetProgress(currentHealth / maxHealth, 3);
    }
    [Rpc(SendTo.Everyone)]
    public void ShowFloatingTextRpc(float damage)
    {
        var go = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity, transform);
        go.GetComponentInChildren<TextMeshPro>().text = damage.ToString();
    }
    [Rpc(SendTo.Everyone)]
    public void ShowFloatingTextCriticRpc(float damage)
    {
        var go = Instantiate(floatingTextCriticPrefab, transform.position, Quaternion.identity, transform);
        go.GetComponentInChildren<TextMeshPro>().text = damage.ToString();
    }
    public void BlinkEffect()
    {
        if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0) return;

        blinkTimer -= Time.deltaTime;

        float lerp = Mathf.Clamp01(blinkTimer / blinkDuration);
        float intensity = (lerp * blinkIntensity) + 1.0f;

        foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
        {
            skinnedMeshRenderer.material.color = Color.white * intensity;
        }
    }
    public void GetPlayer(PlayerControllerMulti player)
    {
        //Debug.Log("Detected player");
        Movement.Player = player.transform;
    }

}
