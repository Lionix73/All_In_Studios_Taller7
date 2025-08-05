using UnityEngine;
using System.Collections.Generic;

public class EnemiesZoneManager : MonoBehaviour
{
    [Header("Zone Management")]
    [SerializeField] private List<Enemy> enemiesInZone = new List<Enemy>();
    [SerializeField] private MoveDoor doorToOpen;
    [SerializeField] private bool autoFindEnemies = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private int initialEnemyCount;
    private bool doorOpened = false;

    public void StartEnemyZone()
    {
        if (autoFindEnemies)
        {
            FindEnemiesInZone();
        }
        
        initialEnemyCount = enemiesInZone.Count;
        
        if (showDebugLogs)
        {
            Debug.Log($"EnemiesZoneManager: Found {initialEnemyCount} enemies in zone");
        }
        
        // Subscribe to enemy death events
        SubscribeToEnemyEvents();
    }

    void Update()
    {
        // Check if all enemies are dead
        CheckIfAllEnemiesEliminated();
    }
    
    private void FindEnemiesInZone()
    {
        Enemy[] allEnemies = GetComponentsInChildren<Enemy>();
        
        enemiesInZone.Clear();
        enemiesInZone.AddRange(allEnemies);
        
        if (enemiesInZone.Count == 0)
        {
            Enemy[] sceneEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            enemiesInZone.AddRange(sceneEnemies);
        }
    }
    
    private void SubscribeToEnemyEvents()
    {
        foreach (Enemy enemy in enemiesInZone)
        {
            if (enemy != null)
            {
                enemy.OnDie += OnEnemyDied;
            }
        }
    }
    
    private void OnEnemyDied(Enemy deadEnemy)
    {
        if (showDebugLogs)
        {
            Debug.Log($"EnemiesZoneManager: Enemy died, checking remaining enemies...");
        }
        
        // Remove the dead enemy from our list
        if (enemiesInZone.Contains(deadEnemy))
        {
            enemiesInZone.Remove(deadEnemy);
        }
        
        // Unsubscribe from the dead enemy's event
        deadEnemy.OnDie -= OnEnemyDied;
    }
    
    private void CheckIfAllEnemiesEliminated()
    {
        if (doorOpened) return;
        
        // Remove any null references (destroyed enemies)
        enemiesInZone.RemoveAll(enemy => enemy == null);
        
        int aliveEnemies = 0;
        foreach (Enemy enemy in enemiesInZone)
        {
            if (enemy != null && !enemy.IsDead)
            {
                aliveEnemies++;
            }
        }
        
        if (aliveEnemies == 0 && initialEnemyCount > 0)
        {
            OpenDoor();
        }
    }
    
    private void OpenDoor()
    {
        if (doorOpened) return;
        
        doorOpened = true;
        
        if (showDebugLogs)
        {
            Debug.Log("EnemiesZoneManager: All enemies eliminated! Opening door...");
        }
        
        if (doorToOpen != null)
        {
            doorToOpen.ToggleDoor();
        }
        else
        {
            Debug.LogWarning("EnemiesZoneManager: No door assigned to open!");
        }
    }
    
    // Public method to manually add enemies to the zone
    public void AddEnemyToZone(Enemy enemy)
    {
        if (enemy != null && !enemiesInZone.Contains(enemy))
        {
            enemiesInZone.Add(enemy);
            enemy.OnDie += OnEnemyDied;
            initialEnemyCount = enemiesInZone.Count;
        }
    }
    
    // Public method to manually remove enemies from the zone
    public void RemoveEnemyFromZone(Enemy enemy)
    {
        if (enemy != null && enemiesInZone.Contains(enemy))
        {
            enemiesInZone.Remove(enemy);
            enemy.OnDie -= OnEnemyDied;
        }
    }
    
    // Public method to get current enemy count
    public int GetAliveEnemyCount()
    {
        int aliveCount = 0;
        foreach (Enemy enemy in enemiesInZone)
        {
            if (enemy != null && !enemy.IsDead)
            {
                aliveCount++;
            }
        }
        return aliveCount;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from all enemy events to prevent memory leaks
        foreach (Enemy enemy in enemiesInZone)
        {
            if (enemy != null)
            {
                enemy.OnDie -= OnEnemyDied;
            }
        }
    }
}
