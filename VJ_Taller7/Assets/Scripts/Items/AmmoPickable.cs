using TMPro;
using UnityEngine;

public class AmmoPickable : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI priceText;

    public GameObject pickeableUI; //En caso de que queramos agregar una descripcion de la cantida que da
    [SerializeField] private TextMeshProUGUI descriptionText;
    private ObjectLookAtCamera uiForLook;
    [SerializeField] private int amountOfAmmo;
    [SerializeField] private float scoreToBuy; private bool canBuy;
    private GunManager playerAmmo;
    private RespawnInteractables respawn;
    public PickeableType typeOfPickable;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerSpawned += GetPlayer;
            GameManager.Instance.ScoreChanged += CheckIfBuyable;
        }

        
        respawn = GetComponentInParent<RespawnInteractables>();
    }
    private void Awake() {
        //descriptionText = pickeableUI.GetComponentInChildren<TextMeshProUGUI>();
        descriptionText.text = $"Get {amountOfAmmo} bullets";
        uiForLook = pickeableUI.GetComponent<ObjectLookAtCamera>();
        pickeableUI.SetActive(false);
    }

    private void Update() {
        if (pickeableUI.activeSelf) uiForLook.LookAtCamera();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(playerAmmo == null)
                playerAmmo = other.transform.root.GetComponentInChildren<GunManager>();
            
            pickeableUI.SetActive(true);
            playerAmmo.EnterPickeableCollectable(typeOfPickable, gameObject);

            //Destroy(gameObject);
            //respawn.StartCountdown(); // ya no queremos respawn
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
        if (!canBuy || playerAmmo.actualTotalAmmo > 599)
        {
            playerAmmo.CannottBuyItem();
            return;
        }

        playerAmmo.actualTotalAmmo += amountOfAmmo;
        playerAmmo.CanBuyItem(typeOfPickable);

        GameManager.Instance.scoreManager.SetScore(-scoreToBuy);
    }

    private void GetPlayer(GameObject player)
    {
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
