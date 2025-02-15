using UnityEngine;
using UnityEngine.AI;

public class Enemy : PoolableObject
{
    public EnemyMovement movement;
    public NavMeshAgent agent;
    [SerializeField] private int Health { get; set; } = 100;

    public override void OnDisable(){
        base.OnDisable();

        agent.enabled = false;
    }
}
