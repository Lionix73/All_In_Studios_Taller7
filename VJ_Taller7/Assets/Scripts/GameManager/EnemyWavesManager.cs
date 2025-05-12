using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class EnemyWavesManager : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Camera mainCamera;

    [Header("Spawn Settings")]
    [SerializeField] private bool storeInitialPos = false;
    [Tooltip("Tamaño inicial de las pools de enemigos")][SerializeField] private int initialPoolSize;
    [Tooltip("Numero inicial de enemigos de oleadas, se cálcula el balance según este número")]
    [SerializeField] private int numberOfEnemiesToSpawn = 1;
    [SerializeField] private float spawnDelay = 1f;
    [SerializeField] private List<WeightedSpawnScriptableObject> weightedEnemies = new List<WeightedSpawnScriptableObject>();
    public List<WeightedSpawnScriptableObject> WeightedEnemies => weightedEnemies;
    [SerializeField] private ScalingScriptableObject scaling;  
    [SerializeField] private SpawnMethod enemySpawnMethod = SpawnMethod.Roundrobin;
    [SerializeField] private bool continousSpawn = false;


    [Space]
    [Header("Read At Runtime")]
    [SerializeField] private int level = 1;
    [SerializeField] private List<EnemyScriptableObject> scaledEnemies = new List<EnemyScriptableObject>();
    [SerializeField] private float[] weights;
    [SerializeField] private int[] availableEnemiesToSpawn;


    private int enemiesAlive = 0;
    private int enemiesSpawned = 0;
    private int initialweightedEnemiesToSpawn;
    private float initialSpawnDelay;

    private NavMeshTriangulation navMeshTriangulation;
    private Dictionary<int, ObjectPool> EnemyObjectPools = new Dictionary<int, ObjectPool>();
    private Dictionary<Enemy, Vector3> initialPositions = new Dictionary<Enemy, Vector3>();

    [Header("UI Settings")]
    [SerializeField] private Canvas healthBarCanvas;

    [Header("Managers")]
    private RoundManager _roundManager;
    private ScoreManager _scoreManager;

    public delegate void EnemySpawned();
    public event EnemySpawned OnEnemySpawned;


    private void Awake()
    {
        ObjectPool.ClearPools();
        GameManager.Instance.PlayerSpawned += GetPlayer;
        OnEnemySpawned += GameManager.Instance.roundManager.enemyHaveSpawn;

        for (int i = 0; i < weightedEnemies.Count; i++)
        {
            EnemyObjectPools.Add(i, ObjectPool.CreateInstance(weightedEnemies[i].enemy.prefab, initialPoolSize));
        }

        weights = new float[weightedEnemies.Count];
        initialweightedEnemiesToSpawn = numberOfEnemiesToSpawn;
        initialSpawnDelay = spawnDelay;
    }

    private void Start()
    {
        _roundManager = GetComponent<RoundManager>();
        _scoreManager = GetComponent<ScoreManager>();
        GameObject.Find("HealthBarCanvas").TryGetComponent(out healthBarCanvas);

        navMeshTriangulation = NavMesh.CalculateTriangulation();

        for (int i = 0; i < weightedEnemies.Count; i++)
        {
            scaledEnemies.Add(weightedEnemies[i].enemy.ScaleUpLevel(scaling, 0));
        }

        if(storeInitialPos){
            StoreInitialPositions();
        }

        //StartCoroutine(SpawnEnemies());
    }

    private void StoreInitialPositions()
    {
        Enemy[] weightedEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in weightedEnemies)
        {
            if (enemy.IsStatic)
            {
                initialPositions[enemy] = enemy.transform.position;
            }
        }
    }

    public void RecieveWaveOrder(int actualWave, int amountOfEnemiesToSpawn){
        //numberOfEnemiesToSpawn = amountOfEnemiesToSpawn;
        //level = actualWave;
        Debug.Log($"Recibiendo oleada de {numberOfEnemiesToSpawn} enemigos");
        ScaleUpSpawns();
        StartCoroutine(SpawnEnemies());
        GameManager.Instance.roundManager.recieveWaveData(numberOfEnemiesToSpawn);

        if (actualWave>2) actualWave = 2;
        availableEnemiesToSpawn = GameManager.Instance.availableEnemiesForWave[actualWave-1].availableEnemies;
    }

    private IEnumerator SpawnEnemies(){
        level++; //Creo que quitare este porque hace la funcion de la current wave

        enemiesAlive = 0;
        enemiesSpawned = 0;

        for (int i = 0; i < weightedEnemies.Count; i++)
        {
            scaledEnemies[i] = weightedEnemies[i].enemy.ScaleUpLevel(scaling, level);
        }

        ResetSpawnWeights(); 

        WaitForSeconds wait = new WaitForSeconds(spawnDelay);

        while(enemiesSpawned < numberOfEnemiesToSpawn){
            if(enemySpawnMethod == SpawnMethod.Roundrobin){
                SpawnRoundRobinEnemy(enemiesSpawned);
            }
            else if(enemySpawnMethod == SpawnMethod.Random){
                SpawnRandomEnemy();
            }
            else if(enemySpawnMethod == SpawnMethod.WeightedRandom){
                SpawnWeightedRandomEnemy();
            }

            yield return wait;
        }

        if(continousSpawn){
            ScaleUpSpawns();
            StartCoroutine(SpawnEnemies());
        }
    }

    private void ResetSpawnWeights(){
        float totalWeight = 0;

        for (int i = 0; i < weightedEnemies.Count; i++)
        {
            weights[i] = weightedEnemies[i].GetWeight();
            totalWeight += weights[i];
        }

        for (int i = 0; i < weights.Length ; i++)
        {
            weights[i] = weights[i]/totalWeight;
        } 
    }

    private void SpawnRoundRobinEnemy(int spawnedweightedEnemies){
        int spawnIndex = spawnedweightedEnemies % weightedEnemies.Count;

        DoSpawnEnemy(spawnIndex, ChooseRandomPositionOnNavMesh());
    }

    private void SpawnRandomEnemy(){
        DoSpawnEnemy(Random.Range(0, weightedEnemies.Count), ChooseRandomPositionOnNavMesh());
    }
    
    private void SpawnWeightedRandomEnemy(){
        float randomValue = Random.value;

        for (int i = 0; i < weights.Length; i++)
        {
            if(randomValue <= weights[i]){
                DoSpawnEnemy(i, ChooseRandomPositionOnNavMesh());
                return;
            }

            randomValue -= weights[i];
        }

        Debug.LogError("No se pudo spawnear un enemigo con peso.");
    }

    private Vector3 ChooseRandomPositionOnNavMesh()
    {
        int VertexIndex = Random.Range(0, navMeshTriangulation.vertices.Length);
        return navMeshTriangulation.vertices[VertexIndex];
    }

    public void DoSpawnEnemy(int spawnIndex, Vector3 spawnPosition){
        if (!availableEnemiesToSpawn.Contains(spawnIndex)) return; //Saber si esta permitido o no ese enemigo

        PoolableObject poolableObject = EnemyObjectPools[spawnIndex].GetObject();

        if(poolableObject != null){
            Enemy enemy = poolableObject.GetComponent<Enemy>();
            scaledEnemies[spawnIndex].SetUpEnemy(enemy);
            
            int vertexIndex = Random.Range(0, navMeshTriangulation.vertices.Length);

            NavMeshHit hit;
            if(NavMesh.SamplePosition(navMeshTriangulation.vertices[vertexIndex], out hit, 2f, -1)){
                enemy.Agent.Warp(hit.position);

                //Enable Collider and Disable Ragdoll
                enemy.RagdollEnabler.EnableAnimator();
                enemy.RagdollEnabler.DisableAllRigidbodies();
                enemy.IsDead = false;

                enemy.MainCamera = mainCamera;
                enemy.Player = player;

                enemy.Movement.Triangulation = navMeshTriangulation;
                enemy.Movement.Player = player.transform;
                enemy.Agent.enabled = true;

                enemy.SetUpHealthBar(healthBarCanvas, mainCamera);


                enemy.Movement.Spawn();
                enemy.OnDie += _roundManager.ChangeScore;
                enemy.OnDie += _roundManager.EnemyDied;
                enemy.OnDie += HandleEnemyDeath;

                enemy.Level = level;
                enemy.Skills = scaledEnemies[spawnIndex].skills;

                enemiesAlive++;
                enemiesSpawned++;
                OnEnemySpawned?.Invoke();
            }
            else{
                Debug.LogError($"No se pudo poner el NavMeshAgent en el navmesh, usando {navMeshTriangulation.vertices[vertexIndex]}");
            }
        }
        else{
            Debug.LogError($"No se logro spawnear un enemigo tipo {spawnIndex} del object pool");
        }
    }

    private void ScaleUpSpawns(){
        numberOfEnemiesToSpawn = Mathf.FloorToInt(initialweightedEnemiesToSpawn * scaling.spawnCountCurve.Evaluate(level + 1));
        spawnDelay = initialSpawnDelay * scaling.spawnRateCurve.Evaluate(level + 1);
    }

    private void HandleEnemyDeath(Enemy enemy){
         enemiesAlive--;

        enemy.OnDie -= HandleEnemyDeath;
        enemy.OnDie -= _roundManager.ChangeScore;
        enemy.OnDie -= _roundManager.EnemyDied;
        if(enemiesAlive == 0 && enemiesSpawned == numberOfEnemiesToSpawn){ //Si la ronda acaba antes del tiempo
            //ScaleUpSpawns(); Ya se escalan cuando se manda la ronda
        }
    }

    public void RespawnEnemy(Enemy enemy)
    {
        if (initialPositions.TryGetValue(enemy, out Vector3 initialPosition))
        {
            enemy.transform.position = initialPosition;
            enemy.gameObject.SetActive(true);
            //enemy.Health = weightedEnemies.Health;
            enemy.SetUpHealthBar(healthBarCanvas, mainCamera);
        }
        else
        {
            Debug.LogError("Initial position not found for the enemy.");
        }
    }

    public enum SpawnMethod{
        Roundrobin,
        Random,
        WeightedRandom
    }

    private void GetPlayer (GameObject activePlayer)
    {
        player = activePlayer.GetComponentInChildren<PlayerController>();
        mainCamera = activePlayer.GetComponentInChildren<Camera>();
    }
}
