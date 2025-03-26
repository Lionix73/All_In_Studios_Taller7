using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.AI;
//using UnityEditor.EditorTools;

public class EnemySpawner : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("Player controller to set as target for the enemies spawned")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Camera mainCamera;

    [Header("Spawn Settings")]
    [Tooltip("If true, the initial position of the enemies will be stored")]
    [SerializeField] private bool storeInitialPos = false; //Actualmente solo usado para el playground
    [SerializeField] private int numberOfEnemiesToSpawn = 1; //Numero de enemigos a spawnear
    [SerializeField] private float spawnDelay = 1f; //Tiempo entre cada spawn
    
    [SerializeField] private List<WeightedSpawnScriptableObject> weightedEnemies = new List<WeightedSpawnScriptableObject>(); //Lista de enemigos con su peso
    public List<WeightedSpawnScriptableObject> WeightedEnemies => weightedEnemies;
    
    [Tooltip("Scriptable object para el scaling de los enemigos")]
    [SerializeField] private ScalingScriptableObject scaling; //Scriptable object para el scaling de los enemigos
    
    [Tooltip("Metodo de spawn de los enemigos, existen 3 metodos: Roundrobin, Random, WeightedRandom")]
    [SerializeField] private SpawnMethod enemySpawnMethod = SpawnMethod.Roundrobin;
    
    [Tooltip("Si se quiere que siga spawneando enemigos despues de que se acaben los enemigos a spawnear")]
    [SerializeField] private bool continousSpawn = false;


    [Space]
    [Header("Read At Runtime")]
    [SerializeField] private int level = 0;

    [Tooltip("Lista de enemigos escalados generados despues de cada nivel")]
    [SerializeField] private List<EnemyScriptableObject> scaledEnemies = new List<EnemyScriptableObject>();
    
    [Tooltip("Pesos de los enemigos para el metodo de spawn WeightedRandom")]
    [SerializeField] private float[] weights;


    private int enemiesAlive = 0;
    private int enemiesSpawned = 0;
    private int initialweightedEnemiesToSpawn;
    private float initialSpawnDelay;

    private NavMeshTriangulation navMeshTriangulation;
    private Dictionary<int, ObjectPool> EnemyObjectPools = new Dictionary<int, ObjectPool>(); //Object pool de los enemigos
    private Dictionary<Enemy, Vector3> initialPositions = new Dictionary<Enemy, Vector3>(); //Posiciones iniciales de los enemigos, SOLO PLAYGROUND

    [Header("UI Settings")]
    [Tooltip("Canvas para el health bar de los enemigos")]
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
            scaledEnemies.Add(weightedEnemies[i].enemy.ScaleUpLevel(scaling, 0)); //Se escala el enemigo al nivel 0
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
            scaledEnemies[i] = weightedEnemies[i].enemy.ScaleUpLevel(scaling, level); //Se escala el enemigo al nivel actual
        }

        ResetSpawnWeights(); //Se resetean los pesos de los enemigos

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
        // Se escoge un vertice aleatorio del navmesh para spawnear al enemigo
        int VertexIndex = Random.Range(0, navMeshTriangulation.vertices.Length);
        return navMeshTriangulation.vertices[VertexIndex];
    }

    public void DoSpawnEnemy(int spawnIndex, Vector3 spawnPosition){
        PoolableObject poolableObject = EnemyObjectPools[spawnIndex].GetObject(); //Se obtiene un enemigo del object pool

        if(poolableObject != null){
            Enemy enemy = poolableObject.GetComponent<Enemy>();
            scaledEnemies[spawnIndex].SetUpEnemy(enemy); //Se setean las propiedades del enemigo
            
            int vertexIndex = Random.Range(0, navMeshTriangulation.vertices.Length); //Se escoge un vertice aleatorio del navmesh para spawnear al enemigo

            NavMeshHit hit;
            if(NavMesh.SamplePosition(navMeshTriangulation.vertices[vertexIndex], out hit, 2f, -1)){
                enemy.Agent.Warp(hit.position); //Se warp el enemigo a la posicion del vertice

                //Enable Collider and Disable Ragdoll
                enemy.RagdollEnabler.EnableAnimator();
                enemy.RagdollEnabler.DisableAllRigidbodies();
                enemy.ColliderEnemy.enabled = true;
                enemy.IsDead = false;

                //Set Enemy Properties
                enemy.MainCamera = mainCamera;
                enemy.Player = player;

                //Set Enemy Movement
                enemy.Movement.Triangulation = navMeshTriangulation;
                enemy.Movement.Player = player.transform;
                enemy.Agent.enabled = true;

                //Set Enemy Health
                enemy.SetUpHealthBar(healthBarCanvas, mainCamera);

                //Set Enemy Skills
                enemy.Movement.Spawn();
                enemy.OnDie += HandleEnemyDeath;
                enemy.Level = level;
                enemy.Skills = scaledEnemies[spawnIndex].skills;

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
        //Se escala el numero de enemigos a spawnear y el tiempo entre cada spawn

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
