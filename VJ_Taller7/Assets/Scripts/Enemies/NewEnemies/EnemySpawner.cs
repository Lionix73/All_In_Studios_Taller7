using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;

    [Header("Spawn Settings")]
    [SerializeField] private bool storeInitialPos = false;
    [SerializeField] private int numberOfEnemiesToSpawn = 1;
    [SerializeField] private float spawnDelay = 1f;
    [SerializeField] private List<WeightedSpawnScriptableObject> weightedEnemies = new List<WeightedSpawnScriptableObject>();
    public List<WeightedSpawnScriptableObject> WeightedEnemies => weightedEnemies;
    [SerializeField] private ScalingScriptableObject scaling;  
    [SerializeField] private SpawnMethod enemySpawnMethod = SpawnMethod.Roundrobin;
    [SerializeField] private bool continousSpawn = false;


    [Space]
    [Header("Read At Runtime")]
    [SerializeField] private int level = 0;
    [SerializeField] private List<EnemyScriptableObject> scaledEnemies = new List<EnemyScriptableObject>();
    [SerializeField] private float[] weights;


    private int enemiesAlive = 0;
    private int enemiesSpawned = 0;
    private int initialweightedEnemiesToSpawn;
    private float initialSpawnDelay;

    private NavMeshTriangulation navMeshTriangulation;
    private Dictionary<int, ObjectPool> EnemyObjectPools = new Dictionary<int, ObjectPool>();
    private Dictionary<Enemy, Vector3> initialPositions = new Dictionary<Enemy, Vector3>();

    [Header("UI Settings")]
    [SerializeField] private Canvas healthBarCanvas;


    private void Awake()
    {
        for (int i = 0; i < weightedEnemies.Count; i++)
        {
            EnemyObjectPools.Add(i, ObjectPool.CreateInstance(weightedEnemies[i].enemy.prefab, numberOfEnemiesToSpawn));
        }

        weights = new float[weightedEnemies.Count];
        initialweightedEnemiesToSpawn = numberOfEnemiesToSpawn;
        initialSpawnDelay = spawnDelay;
    }

    private void Start()
    {
        navMeshTriangulation = NavMesh.CalculateTriangulation();

        for (int i = 0; i < weightedEnemies.Count; i++)
        {
            scaledEnemies.Add(weightedEnemies[i].enemy.ScaleUpLevel(scaling, 0));
        }

        if(storeInitialPos){
            StoreInitialPositions();
        }

        StartCoroutine(SpawnEnemies());
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

    private IEnumerator SpawnEnemies(){
        level++;

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

            enemiesSpawned++;

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
        PoolableObject poolableObject = EnemyObjectPools[spawnIndex].GetObject();

        if(poolableObject != null){
            Enemy enemy = poolableObject.GetComponent<Enemy>();
            scaledEnemies[spawnIndex].SetUpEnemy(enemy);
            
            int vertexIndex = Random.Range(0, navMeshTriangulation.vertices.Length);

            NavMeshHit hit;
            if(NavMesh.SamplePosition(navMeshTriangulation.vertices[vertexIndex], out hit, 2f, -1)){
                enemy.Agent.Warp(hit.position);

                enemy.MainCamera = mainCamera;
                enemy.Movement.Triangulation = navMeshTriangulation;
                enemy.Movement.Player = player;
                enemy.SetUpHealthBar(healthBarCanvas, mainCamera);
                enemy.Agent.enabled = true;
                enemy.Movement.Spawn();
                enemy.OnDie += HandleEnemyDeath;

                enemiesAlive++;
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

        if(enemiesAlive == 0 && enemiesSpawned == numberOfEnemiesToSpawn){
            ScaleUpSpawns();
            StartCoroutine(SpawnEnemies());
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
}
