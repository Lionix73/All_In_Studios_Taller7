using UnityEngine;
using TMPro;
using Unity.Netcode;

public class GunPickeableMulti : NetworkBehaviour
{
    [SerializeField] private GunType gunType;
    [SerializeField] private GameObject PickeableUI;
    [SerializeField] private GameObject priceTagObject; //Por si queremos condicionar cuando se muestre

    [SerializeField] private ObjectLookAtCamera lookCamera;
    [SerializeField] private TextMeshProUGUI gunNameText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI fireRateText;
    [SerializeField] private TextMeshProUGUI magazineText;
    [SerializeField] private TextMeshProUGUI priceText;


    private Vector3 spinDirection = new Vector3(0, 1, 0);
    private GunManagerMulti2 gunManager;
    private NetworkObject player;
    private MultiPlayerState localPlayerState;


    private void Awake()
    {
        //gunName.SetText(gunType.ToString() + "\n" + "Press E to pick up");

        priceTagObject = transform.Find("GunPriceTag").gameObject;

        PickeableUI = GetComponentInChildren<ObjectLookAtCamera>().gameObject;
        lookCamera = PickeableUI.GetComponentInChildren<ObjectLookAtCamera>();

        TextMeshProUGUI[] uiTexts = PickeableUI.GetComponentsInChildren<TextMeshProUGUI>();

        foreach (TextMeshProUGUI text in uiTexts)
        {
            if (text.name == "GunName")
            {
                gunNameText = text;
            }
            else if (text.name == "GunDamage")
            {
                damageText = text;
            }
            else if (text.name == "GunFireRate")
            {
                fireRateText = text;
            }
            else if (text.name == "GunMagazine")
            {
                magazineText = text;
            }
        }
        priceText = priceTagObject.GetComponentInChildren<TextMeshProUGUI>();


        gunNameText.SetText(gunType.ToString());
        PickeableUI.SetActive(false);
    }

    public void TrySubscribeToLocalPlayer(MultiPlayerState player)
    {
        Debug.Log("Suscrito en Pickeables");
        localPlayerState = player;
        gunManager = player.transform.root.GetComponentInChildren<GunManagerMulti2>();
        // Inicializar el precio del arma
        if (gunManager != null)
        {
            GunScriptableObject gun = gunManager.GetGun(gunType);
            priceText.text = $"{gun.scoreToBuy}";
        }
        CheckIfBuyable();
        localPlayerState.OnBuyableWeapons += CheckIfBuyable;
    }

    private void Update()
    {
        transform.Rotate(spinDirection);
    }

    private void OnTriggerEnter(Collider other)
    {
        player = other.GetComponentInParent<NetworkObject>();
        if (player.IsLocalPlayer)
        {
            //gunManager = player.GetComponentInChildren<GunManagerMulti2>();
            //gunManager = other.gameObject.GetComponentInChildren<GunManager>();
            gunManager.EnterPickeableGun(gunType);
            GunScriptableObject gun = gunManager.GetGun(gunType); //Pa tomar lo que quiran del arma a pickear
            damageText.text = $"DMG: {gun.Damage}";
            magazineText.text = $"Magazine: {gun.MagazineSize}";
            PickeableUI.SetActive(true);
            lookCamera = PickeableUI.GetComponentInChildren<ObjectLookAtCamera>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        player = other.GetComponentInParent<NetworkObject>();
        if (player.IsLocalPlayer)
        {
            if (lookCamera != null)
            {
                lookCamera.LookAtCamera();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        player = other.GetComponentInParent<NetworkObject>();
        if (player.IsLocalPlayer)
        {
            PickeableUI.SetActive(false);
            //gunManager = null;

        }
    }

    private void CheckIfBuyable()
    {
        int score = localPlayerState.Score;
        GunScriptableObject gun = gunManager.GetGun(gunType);
        priceText.text = $"{gun.scoreToBuy}"; //necesito porner el score desde el incio y no queria buscar otra vez el gun** 
        priceText.color = score > gun.scoreToBuy ? Color.green : Color.red;
    }

    public override void OnDestroy()
    {
        if(localPlayerState.IsLocalPlayer)
        {
            localPlayerState.OnBuyableWeapons -= CheckIfBuyable;

        }

    }
}
