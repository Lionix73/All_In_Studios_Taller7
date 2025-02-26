using Unity.VisualScripting;
using UnityEngine;

public class HealthPickable : MonoBehaviour
{
    [SerializeField] private float amountOfHealing;
    private Health playerHealth;
    private SoundManager soundManager;

    private void Awake()
    {
        playerHealth = FindAnyObjectByType(typeof(Health)).GetComponent<Health>();
        soundManager = FindAnyObjectByType<SoundManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth.GetHealth = amountOfHealing;
            soundManager.PlaySound("Health");

            Destroy(gameObject);
        }
    }
}
