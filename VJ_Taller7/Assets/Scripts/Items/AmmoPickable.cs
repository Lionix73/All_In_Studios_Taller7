using Unity.VisualScripting;
using UnityEngine;

public class AmmoPickable : MonoBehaviour
{
    [SerializeField] private int amountOfAmmo;
    private GunManager playerAmmo;
    private SoundManager soundManager;

    private void Awake()
    {
        if (GameManager.Instance!=null){
            GameManager.Instance.PlayerSpawned += GetPlayer;
        }
        
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

    private void GetPlayer(GameObject player){
        //playerAmmo = FindAnyObjectByType(typeof(GunManager)).GetComponent<GunManager>();
        playerAmmo = player.GetComponentInChildren<GunManager>();
    }
}
