using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private int numberOfEnemiesToSpawn = 1;
    [SerializeField] private float spawnDelay = 1f;
    [SerializeField] private List<Enemy> enemyPrefabs = new List<Enemy>();
    [SerializeField] private SpawnMethod enemySpawnMethod = SpawnMethod.Roundrobin;

    private NavMeshTriangulation navMeshTriangulation;
    private Dictionary<int, ObjectPool> EnemyObjectPools = new Dictionary<int, ObjectPool>();

    private void Awake()
    {
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            EnemyObjectPools.Add(i, ObjectPool.CreateInstance(enemyPrefabs[i], numberOfEnemiesToSpawn));
        }
    }

    private void Start()
    {
        navMeshTriangulation = NavMesh.CalculateTriangulation();

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

            spawnedEnemies++;

            yield return wait;
        }
    }

    private void SpawnRoundRobinEnemy(int spawnedEnemies){
        int spawnIndex = spawnedEnemies % enemyPrefabs.Count;

        DoSpawnEnemy(spawnIndex);
    }

    private void SpawnRandomEnemy(){
        DoSpawnEnemy(Random.Range(0, enemyPrefabs.Count));
    }

    private void DoSpawnEnemy(int spawnIndex){
        PoolableObject poolableObject = EnemyObjectPools[spawnIndex].GetObject();

        if(poolableObject != null){
            Enemy enemy = poolableObject.GetComponent<Enemy>();
            
            int vertexIndex = Random.Range(0, navMeshTriangulation.vertices.Length);

            NavMeshHit hit;
            if(NavMesh.SamplePosition(navMeshTriangulation.vertices[vertexIndex], out hit, 2f, -1)){
                enemy.Agent.Warp(hit.position);

                enemy.Movement.Player = player;
                enemy.Agent.enabled = true;
                enemy.Movement.StartChasing();
            }
            else{
                Debug.LogError($"No se pudo poner el NavMeshAgent en el navmesh, usando {navMeshTriangulation.vertices[vertexIndex]}");
            }
        }
        else{
            Debug.LogError($"No se logro spawnear un enemigo tipo {spawnIndex} del object pool");
        }
    }

    public enum SpawnMethod{
        Roundrobin,
        Random
    }
}
