using Unity.VisualScripting;
using UnityEngine;

public class AmmoPickable : MonoBehaviour
{
    [SerializeField] private int amountOfAmmo;
    private GunManager playerAmmo;
    private SoundManager soundManager;

    private void Awake()
    {
        playerAmmo = FindAnyObjectByType(typeof(GunManager)).GetComponent<GunManager>();
        soundManager = FindAnyObjectByType<SoundManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerAmmo.actualTotalAmmo += amountOfAmmo;
            soundManager.PlaySound("Ammo");

            Destroy(gameObject);
        }
    }
}
