using Unity.VisualScripting;
using UnityEngine;

public class AmmoPickable : MonoBehaviour
{
    [SerializeField] private int amountOfAmmo;
    ProjectileGuns playerAmmo;

    private void Awake()
    {
        playerAmmo = FindAnyObjectByType(typeof(ProjectileGuns)).GetComponent<ProjectileGuns>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerAmmo.GetTotalAmmo = amountOfAmmo;

            Destroy(gameObject);
        }
    }
}
