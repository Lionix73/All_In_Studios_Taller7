using TMPro;
using UnityEngine;

public class AmmoPickable : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private int amountOfAmmo;
    [SerializeField] private float scoreToBuy; private bool canBuy;
    [SerializeField] private ThisObjectSounds soundManager;
    private GunManager playerAmmo;
    private RespawnInteractables respawn;

    private void OnEnable()
    {
        if (GameManager.Instance!=null){
            GameManager.Instance.PlayerSpawned += GetPlayer;
            GameManager.Instance.ScoreChanged += CheckIfBuyable;
        }
        
        respawn = GetComponentInParent<RespawnInteractables>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canBuy)
        {
            playerAmmo.actualTotalAmmo += amountOfAmmo;
            soundManager?.PlaySound("Ammo");

            //Destroy(gameObject);
            respawn.StartCountdown();
        }
    }

    private void GetPlayer(GameObject player){
        //playerAmmo = FindAnyObjectByType(typeof(GunManager)).GetComponent<GunManager>();
        playerAmmo = player.GetComponentInChildren<GunManager>();

        CheckIfBuyable(1f); //Inicializar interfaz
    }

    private void CheckIfBuyable(float score){
        if (score < scoreToBuy) canBuy = false; 
        else canBuy = true;
        priceText.text = $"{scoreToBuy}"; //necesito porner el score desde el incio y no queria buscar otra vez el gun** 
        priceText.color = score > scoreToBuy ? Color.green: Color.red;
    }
}
