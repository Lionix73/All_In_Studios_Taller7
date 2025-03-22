using UnityEngine;
using TMPro;
using Unity.Netcode;

public class GunPickeableMulti : NetworkBehaviour
{
    [SerializeField] private GunType gunType;
    [SerializeField] private GameObject PickeableUI;
    [SerializeField] private ObjectLookAtCamera lookCamera;
    [SerializeField] private TextMeshProUGUI gunNameText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI fireRateText;
    [SerializeField] private TextMeshProUGUI magazineText;

    private Vector3 spinDirection = new Vector3(0, 1, 0);
    private GunManagerMulti2 gunManager;
    private NetworkObject player;


    private void Awake()
    {
        //gunName.SetText(gunType.ToString() + "\n" + "Press E to pick up");
        gunNameText.SetText(gunType.ToString());
        PickeableUI.SetActive(false);

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
            gunManager = player.GetComponentInChildren<GunManagerMulti2>();
            //gunManager = other.gameObject.GetComponentInChildren<GunManager>();
            gunManager.EnterPickeableGun(gunType);
            GunScriptableObject gun = gunManager.GetGun(gunType); //Pa tomar lo que quiran del arma a pickear
            damageText.text = $"DMG: {gun.Damage}";
            fireRateText.text = $"Fire Rate: {gun.ShootConfig.FireRate}";
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
            gunManager = null;

        }
    }
}
