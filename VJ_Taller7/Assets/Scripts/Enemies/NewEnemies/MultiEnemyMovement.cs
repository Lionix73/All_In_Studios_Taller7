using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent), typeof(AgentLinkMover))]
public class MultiEnemyMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform player;
    public Transform Player{
        get => player;
        set => player = value;
    }

    [SerializeField] private MultiLineOfSightChecker lineOfSightChecker;
    public MultiLineOfSightChecker LineOfSightChecker{
        get => lineOfSightChecker;
        set => lineOfSightChecker = value;
    }

    [SerializeField] private NavMeshTriangulation triangulation;
    public NavMeshTriangulation Triangulation{
        get => triangulation;
        set => triangulation = value;
    }

    [SerializeField] private EnemyMulti enemy;

    [SerializeField] private float updateRate = 0.1f;
    public float UpdateRate{
        get => updateRate;
        set => updateRate = value;
    }
    
    private NavMeshAgent agent;
    private AgentLinkMover linkMover;
    private Coroutine followCoroutine;
    private ThisObjectSounds soundManager;

    [Header("State Settings")]
    [SerializeField] private EnemyState defaultState;
    public EnemyState DefaultState{
        get => defaultState;
        set => defaultState = value;
    }

    private NetworkVariable<EnemyState> networkState = new NetworkVariable<EnemyState>(0);
    //private EnemyState state;
    public EnemyState State{
        get => networkState.Value;
        set{
            if(IsServer)
            {
                OnStateChange?.Invoke(networkState.Value, value);
                networkState.Value = value;
            }
        }
    }

    public delegate void StateChangeEvent(EnemyState oldState, EnemyState newState);
    public StateChangeEvent OnStateChange;

    [SerializeField] private float idleLocationRadius = 4f;
    public float IdleLocationRadius{
        get => idleLocationRadius;
        set => idleLocationRadius = value;
    }

    [SerializeField] private float idleMoveSpeedMultiplier = 0.5f;
    public float IdleMoveSpeedMultiplier{
        get => idleMoveSpeedMultiplier;
        set => idleMoveSpeedMultiplier = value;
    }

    private Vector3[] waypoints = new Vector3[4];
    public Vector3[] Waypoints{
        get => waypoints;
        set => waypoints = value;
    }

    [SerializeField] private int waypointIndex = 0;


    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private FloatDampener speedX;
    [SerializeField] private FloatDampener speedY;

    public const string IsWalking = "IsWalking";
    public const string Jump = "IsJumping";
    public const string Landed = "Landed";

    [Header("NetworkSettings")]
    [SerializeField] private Vector3 lastSyncedPos; // �ltima posici�n sincronizada
    [SerializeField] private Quaternion lastSyncedRot;
    [SerializeField] private float syncThreshold = 0.5f; // Umbral de distancia para sincronizar
    [SerializeField] private float syncInterval = 0.2f; // Intervalo m�nimo entre sincronizaciones
    [SerializeField] private float lastSyncTime; // Tiempo de la �ltima sincronizaci�n
    public NetworkVariable<MultiEnemyAgentConfig> netAgentConfig = new NetworkVariable<MultiEnemyAgentConfig>();

    [Rpc(SendTo.Everyone)]
    public void PlaySoundMultiRpc(string soundName)
    {
        if (IsServer) return;

        soundManager.PlaySound(soundName);
    }

    [Rpc(SendTo.Everyone)]
    public void StopSoundMultiRpc(string soundName)
    {
        if (IsServer) return;

        soundManager.StopSound(soundName);
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        linkMover = GetComponent<AgentLinkMover>();
        soundManager = GetComponent<ThisObjectSounds>();

        linkMover.OnLinkStart += HandleLinkStart;
        linkMover.OnLinkEnd += HandleLinkEnd;

        lineOfSightChecker.OnGainSight += HandleGainSight;
        lineOfSightChecker.OnLoseSight += HandleLoseSight;

        OnStateChange += HandleStateChange;
        netAgentConfig.OnValueChanged += OnAgentConfigChanged;
        snapDistanceThreshold = 1.5f;

        if (enemy != null)
        {
            enemy = GetComponent<EnemyMulti>();
        }
    }
    private void OnAgentConfigChanged(MultiEnemyAgentConfig previous, MultiEnemyAgentConfig current)
    {
        if (!IsServer)
        {
            ApplyAgentConfig(current);
        }
    }

    public void ApplyAgentConfig(MultiEnemyAgentConfig config)
    {
        if (agent != null)
        {
            updateRate = config.updateRate;
            agent.acceleration = config.acceleration;
            agent.angularSpeed = config.angularSpeed;
            agent.areaMask = config.areaMask;
            agent.avoidancePriority = config.avoidancePriority;
            agent.baseOffset = config.baseOffset;
            agent.height = config.height;
            agent.obstacleAvoidanceType = (ObstacleAvoidanceType)config.obstacleAvoidanceType;
            agent.radius = config.radius;
            agent.speed = config.speed;
            agent.stoppingDistance = config.stoppingDistance;
        }
    }
    private void HandleGainSight(HealthMulti player)
    {
        Player = player.transform;
        State = EnemyState.Chase;
    }

    private void HandleLoseSight(HealthMulti player)
    {
        Player = player.transform;
        State = EnemyState.Patrol;
    }

    private void OnDisable()
    {
        State = defaultState;       
    }

    public void Spawn()
    {
        if (!IsServer) return;

        for(int i = 0; i < waypoints.Length; i++){
            NavMeshHit hit;
            if(NavMesh.SamplePosition(triangulation.vertices[Random.Range(0, triangulation.vertices.Length)], out hit, 2f, agent.areaMask)){
                waypoints[i] = hit.position;
            }
            else{
                Debug.LogError("No se pudo encontrar un punto de navmesh cerca de las triangulaciones");
            }
        }

        OnStateChange?.Invoke(EnemyState.Spawn, defaultState);
    }

    private void HandleLinkStart(OffMeshLinkMoveMethod moveMethod)
    {
        if (moveMethod == OffMeshLinkMoveMethod.NormalSpeed)
        {
            //Keep animator in idle state
        }
        else if (moveMethod != OffMeshLinkMoveMethod.Teleport)
        {
            animator.SetTrigger(Jump);
        }
    }

    private void HandleLinkEnd(OffMeshLinkMoveMethod moveMethod)
    {
        if(moveMethod != OffMeshLinkMoveMethod.Teleport && moveMethod != OffMeshLinkMoveMethod.NormalSpeed){
            animator.SetTrigger(Landed);
        }
    }

    private void Update()
    {
        if(!IsServer) return;

        HandleAnims();
    }
    private void HandleAnims()
    {
        if(agent.hasPath && !agent.isOnOffMeshLink){
            Vector3 dir = (agent.steeringTarget - transform.position).normalized;
            Vector3 animDir = transform.InverseTransformDirection(dir);

            speedX.Update();
            speedY.Update();

            speedX.TargetValue = animDir.x;
            speedY.TargetValue = animDir.z;

            animator.SetFloat("Horizontal", speedX.CurrentValue);
            animator.SetFloat("Vertical", speedY.CurrentValue);

            //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), 180 * Time.deltaTime);        
        }
        else{
            animator.SetFloat("Horizontal", 0, 0.25f, Time.deltaTime);
            animator.SetFloat("Vertical", 0, 0.25f, Time.deltaTime);
        }    
    }

    private void HandleStateChange(EnemyState oldState, EnemyState newState)
    {
        if (!IsServer) return;

        if(oldState != newState && gameObject.activeInHierarchy){

            if(followCoroutine != null){
                StopCoroutine(followCoroutine);
            }

            if(oldState == EnemyState.Idle){
                agent.speed /= idleMoveSpeedMultiplier;
            }

            switch(newState){
                case EnemyState.Idle:
                    followCoroutine = StartCoroutine(DoIdleMotion());
                break;

                case EnemyState.Patrol:
                    followCoroutine = StartCoroutine(DoPatrolMotion());
                break;

                case EnemyState.Chase:
                    followCoroutine = StartCoroutine(FollowTarget());
                break;
            }
        }
    }

    private IEnumerator DoIdleMotion()
    {
        soundManager.PlaySound("Idle");
        PlaySoundMultiRpc("Idle");

        WaitForSeconds wait = new WaitForSeconds(updateRate);
        agent.speed *= idleMoveSpeedMultiplier;

        while (!enemy.IsDead)
        {
            if(!agent.enabled || !agent.isOnNavMesh){
                yield return wait;
            }
            else if(agent.remainingDistance <= agent.stoppingDistance){
                Vector2 point = Random.insideUnitCircle * idleLocationRadius;
                NavMeshHit hit;

                if(NavMesh.SamplePosition(agent.transform.position + new Vector3(point.x, 0, point.y), out hit, 2f, agent.areaMask)){
                    SetDestination(hit.position);
                }
            }

            yield return wait;
        }
    }

    private IEnumerator DoPatrolMotion(){
        WaitForSeconds wait = new WaitForSeconds(updateRate);
        
        yield return new WaitUntil(() => agent.isOnNavMesh && agent.enabled);
        SetDestination(waypoints[waypointIndex]);

        while (!enemy.IsDead)
        {
            if(agent.isOnNavMesh && agent.enabled && agent.remainingDistance <= agent.stoppingDistance){
                waypointIndex++;
                
                if(waypointIndex >= waypoints.Length){
                    waypointIndex = 0;
                }

                SetDestination(waypoints[waypointIndex]);
            }

            yield return wait;
        }
    }

    private IEnumerator FollowTarget()
    {
        soundManager.PlaySound("Chase");
        PlaySoundMultiRpc("Chase");

        WaitForSeconds wait = new WaitForSeconds(updateRate);

        while (!enemy.IsDead)
        {

            if(enemy.Health <= 0){
                followCoroutine = null;
                yield break;
            }
            if (player == null) State = EnemyState.Patrol;

            if(agent.enabled){
                SetDestination(player.transform.position);
            }
            yield return wait;
        }
    }

    public void StopMovement()
    {
        soundManager.StopSound("Move");
        StopSoundMultiRpc("Move");

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    public void ResumeMovement()
    {
        soundManager.PlaySound("Move");
        soundManager.StopSound("Idle");
        StopSoundMultiRpc("Idle");

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            agent.ResetPath();

            StartCoroutine(FollowTarget());
            State = EnemyState.Chase;
        }
    }

    public void MoveInCircles()
    {
        float radius = 2f;
        Vector3 randomPoint = GenerateRandomPointInRadius(transform.position, radius);

        if (agent.isOnNavMesh && agent.enabled)
        {
            StopCoroutine(followCoroutine);
            SetDestination(randomPoint);
        }
    }

    public void MoveAround(){
        float radius = 3f;
        if (player != null && agent.isOnNavMesh && agent.enabled)
        {
            Vector3 randomPoint = GenerateRandomPointInRadius(player.position, radius);

            StopCoroutine(followCoroutine); 
            SetDestination(randomPoint);
        }
    }

    private Vector3 GenerateRandomPointInRadius(Vector3 center, float radius)
    {
        Vector2 randomCirclePoint = Random.insideUnitCircle * radius;
        Vector3 randomPoint = new Vector3(center.x + randomCirclePoint.x, center.y, center.z + randomCirclePoint.y);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, radius, agent.areaMask))
        {
            return hit.position;
        }

        return center;
    }

    public void MoveToAttackDistance(float attackRadius)
    {
        if (player == null || enemy.IsDead) return;
        Vector3 direction = (transform.position - player.position).normalized;
        Vector3 targetPosition = player.position + direction * attackRadius;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 2f, agent.areaMask) && agent.isOnNavMesh)
        {
            SetDestination(hit.position);
        }
    }
    
    public void SetDestination(Vector3 hit)
    {
        agent.SetDestination(hit);
        //OnNavMeshStateUpdateClientRpc(transform.position, hit);
        //SetDestinationRpc(hit);
    }

    [Header("Network Sync Settings")]
    [SerializeField] private float snapDistanceThreshold = 1.5f; // Distancia para hacer snap
    [SerializeField] private float lerpSpeed = 10f; // Velocidad de interpolaci�n normal

    [Rpc(SendTo.Everyone)]
    private void OnNavMeshStateUpdateClientRpc(Vector3 serverPosition, Vector3 destination)
    {
        if (IsServer || enemy.IsDead) return;

        // Calcular distancia entre posici�n actual y la del servidor
        float distanceToServer = Vector3.Distance(transform.position, serverPosition);

        // Snap condicional
        if (distanceToServer > snapDistanceThreshold)
        {
            transform.position = new Vector3(serverPosition.x, transform.position.y, serverPosition.z); // Teletransportar si hay gran discrepancia
        }
        else
        {
            // Interpolaci�n suave para diferencias peque�as
            transform.position = Vector3.Lerp(transform.position, new Vector3 (serverPosition.x , transform.position.y , serverPosition.z), lerpSpeed * Time.deltaTime);
        }

        // Actualizar agente de navegaci�n
        agent.SetDestination(destination);
    }

    private void OnDrawGizmosSelected()
    {
        for(int i = 0; i < waypoints.Length; i++){
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(waypoints[i], 0.25f);

            if(i + 1 < waypoints.Length){
                Gizmos.color = Color.green;
                Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
            }
            else{
                Gizmos.color = Color.green;
                Gizmos.DrawLine(waypoints[i], waypoints[0]);
            }
        }
    }
}
