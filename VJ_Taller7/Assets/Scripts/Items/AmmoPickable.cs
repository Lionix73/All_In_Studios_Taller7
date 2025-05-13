using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;

public class AmmoPickable : MonoBehaviour
{
    [SerializeField] private int amountOfAmmo;
    [SerializeField] private ThisObjectSounds soundManager;
    private GunManager playerAmmo;
    private RespawnInteractables respawn;

    private void OnEnable()
    {
        if (GameManager.Instance!=null){
            GameManager.Instance.PlayerSpawned += GetPlayer;
        }
        
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
