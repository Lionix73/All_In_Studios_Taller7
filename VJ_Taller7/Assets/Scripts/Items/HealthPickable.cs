using Unity.VisualScripting;
using UnityEngine;

public class HealthPickable : MonoBehaviour
{
    [SerializeField] private float amountOfHealing;
    private Health playerHealth;
    private SoundManager soundManager;
    private RespawnInteractables respawn;

    private void Awake()
    {
        if (GameManager.Instance!=null){
            GameManager.Instance.PlayerSpawned += GetPlayer;
        }
        
        soundManager = FindAnyObjectByType<SoundManager>();
        respawn = GetComponentInParent<RespawnInteractables>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth.TakeHeal(amountOfHealing);
            soundManager.PlaySound("Health");

            //Destroy(gameObject);
            respawn.StartCountdown();
        }
    }

    private void GetPlayer(GameObject player){
        //playerHealth = FindAnyObjectByType(typeof(Health)).GetComponent<Health>();
        playerHealth = player.GetComponentInChildren<Health>();
    }
}
