using Unity.VisualScripting;
using TMPro;
using UnityEngine;

public class HealthPickable : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI priceText;
    public GameObject pickeableUI;
    [SerializeField] private TextMeshProUGUI descriptionText;
    private ObjectLookAtCamera uiForLook;
    [SerializeField] private float scoreToBuy; private bool canBuy;
    [SerializeField] private float amountOfHealing;
    [SerializeField] private ThisObjectSounds soundManager;
    private Health playerHealth;
    private GunManager playerAmmo; //referencia igual a la del ammo pickeable para manegar todo ahi
    private RespawnInteractables respawn;
    public PickeableType typeOfPickable;
    

    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerSpawned += GetPlayer;
            GameManager.Instance.ScoreChanged += CheckIfBuyable;
        }

        //''descriptionText = pickeableUI.GetComponentInChildren<TextMeshProUGUI>();
        descriptionText.text = $"Get {amountOfHealing} points of health";
        uiForLook = pickeableUI.GetComponent<ObjectLookAtCamera>();
        pickeableUI.SetActive(false);
        respawn = GetComponentInParent<RespawnInteractables>();
    }

    private void Update()
    {
        if (pickeableUI.activeSelf) uiForLook.LookAtCamera();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pickeableUI.SetActive(true);
            playerAmmo.EnterPickeableCollectable(typeOfPickable, gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pickeableUI.SetActive(false);
            playerAmmo.ExitPickeableCollectable(); 
        }
    }

    public void BuyCollectable()
    {
        if (!canBuy)
        {
            soundManager.PlaySound("CantBuyItem");
            return;
        }

        if (playerHealth.GetCurrentHeath == playerHealth.GetMaxHeath) return; //Evitar curar sin queres al tener toda la vida
        playerHealth.TakeHeal(amountOfHealing);
        soundManager?.PlaySound("HalthPickable");

        GameManager.Instance.scoreManager.SetScore(-scoreToBuy);
    }

    private void GetPlayer(GameObject player)
    {
        //playerHealth = FindAnyObjectByType(typeof(Health)).GetComponent<Health>();
        playerHealth = player.GetComponentInChildren<Health>();
        playerAmmo = player.GetComponentInChildren<GunManager>();
        CheckIfBuyable(1f);
    }
    
     private void CheckIfBuyable(float score){
        if (score < scoreToBuy) canBuy = false; 
        else canBuy = true;
        priceText.text = $"{scoreToBuy}"; //necesito porner el score desde el incio y no queria buscar otra vez el gun** 
        priceText.color = score > scoreToBuy ? Color.green: Color.red;
    }
}
