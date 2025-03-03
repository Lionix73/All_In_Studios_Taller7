using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBurstSpawnArea : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private List<EnemyScriptableObject> enemies = new List<EnemyScriptableObject>();
    [SerializeField] private EnemySpawner.SpawnMethod spawnMethod = EnemySpawner.SpawnMethod.Random;
    [SerializeField] private int spawnCount = 10;
    [SerializeField] private float spawnDelay = 0.5f;

    private Coroutine spawnEnemieCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if(spawnEnemieCoroutine == null){
            spawnEnemieCoroutine = StartCoroutine(SpawnEnemies());
        }
    }

    private IEnumerator SpawnEnemies()
    {
        WaitForSeconds wait = new WaitForSeconds(spawnDelay);

        for (int i = 0; i < spawnCount; i++)
        {
            if(spawnMethod == EnemySpawner.SpawnMethod.Roundrobin){
                enemySpawner.DoSpawnEnemy(enemySpawner.Enemies.FindIndex((enemy) => enemy.Equals(enemies[i % enemies.Count])));
            } else if (spawnMethod == EnemySpawner.SpawnMethod.Random){
                int index = Random.Range(0, enemies.Count);
                enemySpawner.DoSpawnEnemy(enemySpawner.Enemies.FindIndex((enemy) => enemy.Equals(enemies[index])));
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
