using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class AmmoPickableMulti : NetworkBehaviour
{
    [SerializeField] private int amountOfAmmo;
    private GunManagerMulti2 playerAmmo;
    private SoundManager soundManager;

    private void Awake()
    {
        
        //soundManager = FindAnyObjectByType<SoundManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NetworkObject player = other.GetComponentInParent<NetworkObject>();
            if (IsServer)
            {
                playerAmmo = player.GetComponentInChildren<GunManagerMulti2>();
                playerAmmo.GainAmmo(amountOfAmmo);
                GetComponent<NetworkObject>().Despawn();
            }
            soundManager.PlaySound("Ammo");
            Destroy(gameObject);
        }
    }

}
