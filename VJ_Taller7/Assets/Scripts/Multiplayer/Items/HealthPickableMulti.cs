using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class HealthPickableMulti : NetworkBehaviour
{
    [SerializeField] private int amountOfHealing;
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
                GetComponent<NetworkObject>().Despawn();
            }
                soundManager.PlaySound("Health");
                Destroy(gameObject);
        }
    }

    private void GetPlayer(GameObject player)
    {
        //playerHealth = FindAnyObjectByType(typeof(Health)).GetComponent<Health>();
        playerHealth = player.GetComponentInChildren<HealthMulti>();
    }
}
