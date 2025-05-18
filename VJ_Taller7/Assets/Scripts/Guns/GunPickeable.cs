using UnityEngine;
using TMPro;

public class GunPickeable : MonoBehaviour
{
    [SerializeField] private GunType gunType;
    [SerializeField] private GameObject PickeableUI;
    [SerializeField] private GameObject priceTagObject; //Por si queremos condicionar cuando se muestre
    [SerializeField] private GameObject GunModel;
    [SerializeField] private ObjectLookAtCamera lookCamera;
    [SerializeField] private TextMeshProUGUI gunNameText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI fireRateText;
    [SerializeField] private TextMeshProUGUI magazineText;
    [SerializeField] private TextMeshProUGUI priceText;

    [SerializeField] private Vector3 spinDirection = new Vector3(0, 1, 0);
    private GunManager gunManager;


    private void Awake() {
        //gunName.SetText(gunType.ToString() + "\n" + "Press E to pick up");

        GameManager.Instance.ScoreChanged += CheckIfBuyable;
        GameManager.Instance.PlayerSpawned += GetPlayer;
        
        priceTagObject = transform.Find("GunPriceTag").gameObject;

        PickeableUI = GetComponentInChildren<ObjectLookAtCamera>().gameObject;
        lookCamera = PickeableUI.GetComponentInChildren<ObjectLookAtCamera>();

        TextMeshProUGUI[] uiTexts = PickeableUI.GetComponentsInChildren<TextMeshProUGUI>();

        foreach (TextMeshProUGUI text in uiTexts) {
            if (text.name == "GunName") {
                gunNameText = text;
            } else if (text.name == "GunDamage") {
                damageText = text;
            } else if (text.name == "GunFireRate") {
                fireRateText = text;
            } else if (text.name == "GunMagazine") {
                magazineText = text;
            }
        }

        priceText = priceTagObject.GetComponentInChildren<TextMeshProUGUI>();


        gunNameText.SetText(gunType.ToString());
        PickeableUI.SetActive(false);
        
    }

    private void Update() {
        GunModel.transform.Rotate(spinDirection);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            
            //gunManager = other.gameObject.GetComponentInChildren<GunManager>();
            gunManager.EnterPickeableGun(gunType);
            GunScriptableObject gun = gunManager.GetGun(gunType); //Pa tomar lo que quiran del arma a pickear
            damageText.text = $"DMG: {gun.Damage}";
            fireRateText.text = $"Fire Rate: {gun.ShootConfig.FireRate}";
            magazineText.text = $"Magazine: {gun.MagazineSize}";
            

            //Pricetext.text= "gun.scoreToBuy" //Falta ponerlo de verdad
            PickeableUI.SetActive(true);
            //lookCamera = PickeableUI.GetComponentInChildren<ObjectLookAtCamera>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (lookCamera !=null)
            {
                lookCamera.LookAtCamera();
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            PickeableUI.SetActive(false);
        }
    }

    private void GetPlayer(GameObject player){
        gunManager = player.GetComponentInChildren<GunManager>();
        CheckIfBuyable(1f); //simplemente inicializar la interfaz de costo costo
    }

    private void CheckIfBuyable(float score){
        GunScriptableObject gun = gunManager.GetGun(gunType);
        priceText.text = $"{gun.scoreToBuy}"; //necesito porner el score desde el incio y no queria buscar otra vez el gun** 
        priceText.color = score > gun.scoreToBuy ? Color.green: Color.red;
    }
}
