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
    [SerializeField] private Vector3 lastSyncedPos; // Última posición sincronizada
    [SerializeField] private Quaternion lastSyncedRot;
    [SerializeField] private float syncThreshold = 0.5f; // Umbral de distancia para sincronizar
    [SerializeField] private float syncInterval = 0.2f; // Intervalo mínimo entre sincronizaciones
    [SerializeField] private float lastSyncTime; // Tiempo de la última sincronización

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        linkMover = GetComponent<AgentLinkMover>();

        linkMover.OnLinkStart += HandleLinkStart;
        linkMover.OnLinkEnd += HandleLinkEnd;

        lineOfSightChecker.OnGainSight += HandleGainSight;
        lineOfSightChecker.OnLoseSight += HandleLoseSight;

        OnStateChange += HandleStateChange;

        if (enemy != null)
        {
            enemy = GetComponent<EnemyMulti>();
        }
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();  
    }

    private void HandleGainSight(PlayerControllerMulti player)
    {
        Player = player.transform;
        State = EnemyState.Chase;
    }

    private void HandleLoseSight(PlayerControllerMulti player)
    {
        State = EnemyState.Patrol;
    }

    private void OnDisable()
    {
        State = defaultState;       
    }

    public void Spawn()
    {
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
        if (!IsServer) return;
        if (Vector3.Distance(transform.position, lastSyncedPos) > 0.01f)
        {
            lastSyncedPos = transform.position;
            lastSyncedRot = transform.rotation;
           SyncPositionClientRpc(lastSyncedPos, lastSyncedRot);
        }

        HandleAnims();

    }
    [Rpc(SendTo.Everyone)]
    private void SyncPositionClientRpc(Vector3 newPosition, Quaternion newRotation)
    {
        if (!IsServer) // Solo clientes actualizan su posición
        {
            transform.position = newPosition;
            transform.rotation = newRotation;
        }
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

    private IEnumerator DoIdleMotion(){
        WaitForSeconds wait = new WaitForSeconds(updateRate);
        agent.speed *= idleMoveSpeedMultiplier;

        while (true)
        {
            if(!agent.enabled || !agent.isOnNavMesh){
                yield return wait;
            }
            else if(agent.remainingDistance <= agent.stoppingDistance){
                Vector2 point = Random.insideUnitCircle * idleLocationRadius;
                NavMeshHit hit;

                if(NavMesh.SamplePosition(agent.transform.position + new Vector3(point.x, 0, point.y), out hit, 2f, agent.areaMask)){
                    agent.SetDestination(hit.position);
                }
            }

            yield return wait;
        }
    }

    private IEnumerator DoPatrolMotion(){
        WaitForSeconds wait = new WaitForSeconds(updateRate);
        
        yield return new WaitUntil(() => agent.isOnNavMesh && agent.enabled);
        agent.SetDestination(waypoints[waypointIndex]);

        while (true)
        {
            if(agent.isOnNavMesh && agent.enabled && agent.remainingDistance <= agent.stoppingDistance){
                waypointIndex++;
                
                if(waypointIndex >= waypoints.Length){
                    waypointIndex = 0;
                }

                agent.SetDestination(waypoints[waypointIndex]);
            }

            yield return wait;
        }
    }

    private IEnumerator FollowTarget(){
        WaitForSeconds wait = new WaitForSeconds(updateRate);

        while (true){

            if(enemy.Health <= 0){
                followCoroutine = null;
                yield break;
            }
            if (player == null) State = EnemyState.Patrol;

            if(agent.enabled){
                agent.SetDestination(player.transform.position);
            }
            yield return wait;
        }
    }

    public void StopMovement(){
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    public void ResumeMovement(){
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
            agent.SetDestination(randomPoint);
        }
    }

    public void MoveAround(){
        float radius = 3f;
        if (player != null && agent.isOnNavMesh && agent.enabled)
        {
            Vector3 randomPoint = GenerateRandomPointInRadius(player.position, radius);

            StopCoroutine(followCoroutine); 
            agent.SetDestination(randomPoint);
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
            agent.SetDestination(hit.position);
        }
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
