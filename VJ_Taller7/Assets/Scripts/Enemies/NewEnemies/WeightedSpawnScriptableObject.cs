using UnityEngine;

[CreateAssetMenu(fileName = "Weighted Spawn Config", menuName = "Enemies/Weighted Spawn Config")]
public class WeightedSpawnScriptableObject : ScriptableObject
{
    public EnemyScriptableObject enemy;
    [Range(0, 1)] public float minWeight;
    [Range(0, 1)] public float maxWeight;

    public float GetWeight()
    {
        return Random.Range(minWeight, maxWeight);
    }
}
