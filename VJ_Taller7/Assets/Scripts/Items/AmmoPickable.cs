using Unity.VisualScripting;
using UnityEngine;

public class AmmoPickable : MonoBehaviour
{
    [SerializeField] private int amountOfAmmo;
    private GunManager playerAmmo;
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
            playerAmmo.actualTotalAmmo += amountOfAmmo;
            soundManager.PlaySound("Ammo");

            //Destroy(gameObject);
            respawn.StartCountdown();
        }
    }

    private void GetPlayer(GameObject player){
        //playerAmmo = FindAnyObjectByType(typeof(GunManager)).GetComponent<GunManager>();
        playerAmmo = player.GetComponentInChildren<GunManager>();
    }
}
