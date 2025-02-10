using Unity.VisualScripting;
using UnityEngine;

public class HealthPickable : MonoBehaviour
{
    [SerializeField] private float amountOfHealing;
    Health playerHealth;
    private void Awake()
    {
        playerHealth = FindAnyObjectByType(typeof(Health)).GetComponent<Health>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth.GetHealth = amountOfHealing;

            Destroy(gameObject);
        }
    }
}
