using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class HealthPickableMulti : NetworkBehaviour
{
    [SerializeField] private float amountOfHealing;
    private HealthMulti playerHealth;
    private SoundManager soundManager;

    private void Awake()
    {

        soundManager = FindAnyObjectByType<SoundManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (IsServer)
            {
                playerHealth = other.GetComponent<HealthMulti>();
                playerHealth.TakeHeal(amountOfHealing);
                playerHealth.HealthChangeRpc();
                GetComponent<NetworkObject>().Despawn();
            }
                soundManager.PlaySound("Health");
                Destroy(gameObject);
        }
    }
}
