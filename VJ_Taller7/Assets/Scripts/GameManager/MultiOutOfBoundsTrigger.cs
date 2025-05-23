using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MultiOutOfBoundsTrigger : NetworkBehaviour
{
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private float damageMultiplier = 0.5f; // Porcentaje de daño a aplicar

    private void Start()
    {
        if (spawnPoint == null)
        {
            spawnPoint = GameObject.FindGameObjectWithTag("Respawn");
        }
    }
    private HashSet<HealthMulti> playersInside = new HashSet<HealthMulti>();

    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent<HealthMulti>(out var playerHealth))
        {
            playersInside.Add(playerHealth);
            ApplyDamage(playerHealth);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent<HealthMulti>(out var playerHealth))
        {
            playersInside.Remove(playerHealth);
        }
    }

    void ApplyDamage(HealthMulti playerHealth)
    {
        MultiPlayerState playerState = playerHealth.GetComponent<MultiPlayerState>();
        playerState.RespawnPlayer(spawnPoint.transform);
        int damageToDeal = Mathf.FloorToInt(playerHealth.CurrentHealth * 0.5f);
        Debug.Log(damageToDeal);
        playerHealth.TakeDamage(damageToDeal, 10);
        //StartCoroutine(RespawnPlayer(playerHealth.gameObject));
    }
}
