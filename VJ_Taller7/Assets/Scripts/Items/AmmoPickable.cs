using Unity.VisualScripting;
using UnityEngine;

public class AmmoPickable : MonoBehaviour
{
    [SerializeField] private int amountOfAmmo;
    GunManager playerAmmo;

    private void Awake()
    {
        playerAmmo = FindAnyObjectByType(typeof(GunManager)).GetComponent<GunManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerAmmo.actualTotalAmmo += amountOfAmmo;

            Destroy(gameObject);
        }
    }
}
