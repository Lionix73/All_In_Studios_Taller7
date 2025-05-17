using System.Collections.Generic;
using UnityEngine;

public class TimePartitionManager : MonoBehaviour
{
    private static TimePartitionManager _instance;
    public static TimePartitionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("TimePartitionManager");
                _instance = go.AddComponent<TimePartitionManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [Tooltip("Number of frames to distribute enemy updates across")]
    [SerializeField] private int partitionCount = 4;
    
    [Tooltip("Maximum number of enemies that can update in a single frame")]
    [SerializeField] private int maxUpdatesPerFrame = 10;
    
    // Lists of enemies for each partition
    private List<Enemy>[] partitions;
    
    // Current partition being processed
    private int currentPartition = 0;
    
    // Enemies waiting to be assigned to a partition
    private Queue<Enemy> pendingEnemies = new Queue<Enemy>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializePartitions();
    }

    private void InitializePartitions()
    {
        partitions = new List<Enemy>[partitionCount];
        for (int i = 0; i < partitionCount; i++)
        {
            partitions[i] = new List<Enemy>();
        }
    }

    private void Update()
    {
        // Process any pending enemies
        ProcessPendingEnemies();
        
        // Update the current partition
        UpdateCurrentPartition();
        
        // Move to the next partition for the next frame
        currentPartition = (currentPartition + 1) % partitionCount;
    }

    private void ProcessPendingEnemies()
    {
        // Find the partition with the fewest enemies
        int smallestPartitionIndex = 0;
        int smallestPartitionSize = int.MaxValue;
        
        for (int i = 0; i < partitionCount; i++)
        {
            if (partitions[i].Count < smallestPartitionSize)
            {
                smallestPartitionSize = partitions[i].Count;
                smallestPartitionIndex = i;
            }
        }
        
        // Add pending enemies to the smallest partition
        int count = Mathf.Min(pendingEnemies.Count, maxUpdatesPerFrame);
        for (int i = 0; i < count; i++)
        {
            if (pendingEnemies.Count > 0)
            {
                Enemy enemy = pendingEnemies.Dequeue();
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    partitions[smallestPartitionIndex].Add(enemy);
                    smallestPartitionIndex = (smallestPartitionIndex + 1) % partitionCount;
                }
            }
        }
    }

    private void UpdateCurrentPartition()
    {
        List<Enemy> currentEnemies = partitions[currentPartition];
        
        // Clean up inactive enemies
        currentEnemies.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);
        
        // Update active enemies in current partition
        for (int i = 0; i < currentEnemies.Count; i++)
        {
            if (currentEnemies[i] != null && currentEnemies[i].gameObject.activeInHierarchy)
            {
                // Process AI logic
                currentEnemies[i].ProcessAI();
                
                // Process movement
                if (currentEnemies[i].Movement != null)
                {
                    currentEnemies[i].Movement.ProcessMovement();
                }
            }
        }
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if (enemy != null && !pendingEnemies.Contains(enemy))
        {
            pendingEnemies.Enqueue(enemy);
        }
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        for (int i = 0; i < partitionCount; i++)
        {
            partitions[i].Remove(enemy);
        }
    }
}