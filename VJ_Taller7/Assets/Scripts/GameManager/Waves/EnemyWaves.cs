using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemyWaves : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;

    [Header("Spawn Settings")]
    [SerializeField] private bool storeInitialPos = false;
    [field:SerializeField] private int numberOfEnemiesToSpawn=1;
    [SerializeField] private float spawnDelay = 1f;
    [Tooltip("Enemigos disponibles en el juego")]
    [SerializeField] private List<EnemyScriptableObject> enemies = new List<EnemyScriptableObject>();
    [Tooltip("Indices de los enemigos que van a aparecer")] 
    private List<int> enemiesIndex;
    [SerializeField] private SpawnMethod enemySpawnMethod = SpawnMethod.Roundrobin;

    [Header("UI Settings")]
    [SerializeField] private Canvas healthBarCanvas;

    private NavMeshTriangulation navMeshTriangulation;
    private Dictionary<int, ObjectPool> EnemyObjectPools = new Dictionary<int, ObjectPool>();
    private Dictionary<Enemy, Vector3> initialPositions = new Dictionary<Enemy, Vector3>();

    [Header("Managers")]
    private RoundManager _roundManager;
    private ScoreManager _scoreManager;

    private void Start()
    {
        _roundManager = GetComponent<RoundManager>();
        _scoreManager = GetComponent<ScoreManager>();

        navMeshTriangulation = NavMesh.CalculateTriangulation();

        if(storeInitialPos){
            StoreInitialPositions();
        }
    }

    public void SetEnemiesInSpawner(List<BuyableEnemy> available){
        for (int i = 0; i < available.Count; i++){
            enemies.Add(available[i].enemyInfo);
        }

        //Si somos absolutamente precisos debería buscar por cada index y crear la cantidad... pero pa luego :D
        //primera pool
        for (int i = 0; i < enemies.Count; i++){
            EnemyObjectPools.Add(i, ObjectPool.CreateInstance(enemies[i].prefab, numberOfEnemiesToSpawn));
        }
    }

    //[ContextMenu("CreateEnemyPool")]
    //private void CreateEnemyPool(){
    //    ObjectPool poolCheck;
    //    for (int i = 0; i < enemies.Count; i++)
    //    {
    //        if (EnemyObjectPools.TryGetValue(i, out poolCheck)){
    //            if (poolCheck.Size < enemies.Count){
    //            }
    //        }
    //        else continue;
    //    }
    //}

    private void StoreInitialPositions()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            if (enemy.IsStatic)
            {
                initialPositions[enemy] = enemy.transform.position;
            }
        }
    }

    public void RecibeWaveEnemies(List<int> index){
        numberOfEnemiesToSpawn = index.Count;
        enemiesIndex = index;
        //CreateEnemyPool(); //Todavía estoy viendo como hacerla dinámica, por ahora, solo es muy grande
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies(){
        WaitForSeconds wait = new WaitForSeconds(spawnDelay);

        int spawnedEnemies = 0;

        while(spawnedEnemies < numberOfEnemiesToSpawn){
            if(enemySpawnMethod == SpawnMethod.Roundrobin){
                SpawnRoundRobinEnemy(spawnedEnemies);
            }
            else if(enemySpawnMethod == SpawnMethod.Random){
                SpawnRandomEnemy();
            }
            else if (enemySpawnMethod == SpawnMethod.Especific){
                SpawnEspecificEnemy(enemiesIndex[spawnedEnemies]);
            }

            spawnedEnemies++;

            yield return wait;
        }
    }

    private void SpawnRoundRobinEnemy(int spawnedEnemies){
        int spawnIndex = spawnedEnemies % enemies.Count;

        DoSpawnEnemy(spawnIndex);
    }

    private void SpawnEspecificEnemy(int index){
        DoSpawnEnemy(index);
    }

    private void SpawnRandomEnemy(){
        DoSpawnEnemy(Random.Range(0, enemies.Count));
    }

    private void DoSpawnEnemy(int spawnIndex){
        PoolableObject poolableObject = EnemyObjectPools[spawnIndex].GetObject();

        if(poolableObject != null){
            Enemy enemy = poolableObject.GetComponent<Enemy>();

            enemy.OnEnemyDead += _roundManager.EnemyDied; //suscribir al evento
            enemy.OnEnemyDead += _roundManager.ChangeScore; //suscribir al evento
            
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
            }
            else{
                Debug.LogError($"No se pudo poner el NavMeshAgent en el navmesh, usando {navMeshTriangulation.vertices[vertexIndex]}");
            }
        }
        else{
            Debug.LogError($"No se logro spawnear un enemigo tipo {spawnIndex} del object pool");
        }
    }

    public void RespawnEnemy(Enemy enemy)
    {
        if (initialPositions.TryGetValue(enemy, out Vector3 initialPosition))
        {
            enemy.transform.position = initialPosition;
            enemy.gameObject.SetActive(true);
            //enemy.Health = enemies.Health;
            enemy.SetUpHealthBar(healthBarCanvas, mainCamera);
        }
        else
        {
            Debug.LogError("Initial position not found for the enemy.");
        }
    }

    public void ChangeScore(object sender, Enemy.OnEnemyDeadEventArgs e){

    }

    public enum SpawnMethod{
        Roundrobin,
        Random,
        Especific
    }
}
