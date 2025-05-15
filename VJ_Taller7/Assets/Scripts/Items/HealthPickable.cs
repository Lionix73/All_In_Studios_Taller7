using Unity.VisualScripting;
using TMPro;
using UnityEngine;

public class HealthPickable : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private float scoreToBuy; private bool canBuy;
    [SerializeField] private float amountOfHealing;
    [SerializeField] private ThisObjectSounds soundManager;
    private Health playerHealth;
    private RespawnInteractables respawn;

    private void Awake()
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
            if (playerHealth.GetCurrentHeath == playerHealth.GetMaxHeath) return; //Evitar curar sin queres al pasar por encima sobre todo en las primeras rondas
            playerHealth.TakeHeal(amountOfHealing);
            soundManager?.PlaySound("Health");

            //Destroy(gameObject);
            respawn.StartCountdown();
        }
    }

    private void GetPlayer(GameObject player){
        //playerHealth = FindAnyObjectByType(typeof(Health)).GetComponent<Health>();
        playerHealth = player.GetComponentInChildren<Health>();
        CheckIfBuyable(1f);
    }
    
     private void CheckIfBuyable(float score){
        if (score < scoreToBuy) canBuy = false; 
        else canBuy = true;
        priceText.text = $"{scoreToBuy}"; //necesito porner el score desde el incio y no queria buscar otra vez el gun** 
        priceText.color = score > scoreToBuy ? Color.green: Color.red;
    }
}
