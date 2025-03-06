using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBurstSpawnArea : MonoBehaviour
{
    [SerializeField] private Collider SpawnCollider;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private List<EnemyScriptableObject> enemies = new List<EnemyScriptableObject>();
    [SerializeField] private EnemySpawner.SpawnMethod spawnMethod = EnemySpawner.SpawnMethod.Random;
    [SerializeField] private int spawnCount = 10;
    [SerializeField] private float spawnDelay = 0.5f;

    private Coroutine spawnEnemieCoroutine;
    private Bounds bounds;

    private void Awake()
    {
        if (SpawnCollider == null)
        {
            SpawnCollider = GetComponent<Collider>();
        }
        
        bounds = SpawnCollider.bounds;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(spawnEnemieCoroutine == null){
            spawnEnemieCoroutine = StartCoroutine(SpawnEnemies());
        }
    }

    private Vector3 GetRandomPositionInBounds()
    {
        return new Vector3(Random.Range(bounds.min.x, bounds.max.x), bounds.min.y, Random.Range(bounds.min.z, bounds.max.z));
    }


    private IEnumerator SpawnEnemies()
    {
        WaitForSeconds wait = new WaitForSeconds(spawnDelay);

        for (int i = 0; i < spawnCount; i++)
        {
            if(spawnMethod == EnemySpawner.SpawnMethod.Roundrobin){
                enemySpawner.DoSpawnEnemy(enemySpawner.WeightedEnemies.FindIndex((enemy) => enemy.enemy.Equals(enemies[i % enemies.Count])), GetRandomPositionInBounds());
            } else if (spawnMethod == EnemySpawner.SpawnMethod.Random){
                int index = Random.Range(0, enemies.Count);
                enemySpawner.DoSpawnEnemy(enemySpawner.WeightedEnemies.FindIndex((enemy) => enemy.enemy.Equals(enemies[index])), GetRandomPositionInBounds());
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
