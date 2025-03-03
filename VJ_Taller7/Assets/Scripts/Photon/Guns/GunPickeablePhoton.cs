using UnityEngine;
using TMPro;
using Fusion;

public class GunPickeablePhoton : NetworkBehaviour
{
    [SerializeField] private GunType gunType;
    [SerializeField] private GameObject PickeableUI;
    [SerializeField] private ObjectLookAtCamera lookCamera;
    [SerializeField] private TextMeshProUGUI gunNameText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI fireRateText;
    [SerializeField] private TextMeshProUGUI magazineText;

    [SerializeField] private GameObject gunModel;
    private Vector3 spinDirection = new Vector3(0, 0, 1);
    private GunManager gunManager;
    public PlayerControllerPhoton LocalPlayer;
    // Networked Property para sincronizar el estado de la UI
    [Networked,OnChangedRender(nameof(UpdatePickeableUIState))]private NetworkBool IsPickeableUIActive { get; set; }

    private void Awake() {
        //gunName.SetText(gunType.ToString() + "\n" + "Press E to pick up");
        gunNameText.SetText(gunType.ToString());
        PickeableUI.SetActive(false);
        //gunModel = GameObject.FindWithTag("GunModels");

    }

    public override void FixedUpdateNetwork() {
        gunModel.transform.Rotate(spinDirection);
    }

    private void OnTriggerEnter(Collider other) {
        LocalPlayer = other.GetComponentInParent<PlayerControllerPhoton>();
        if (LocalPlayer != null && LocalPlayer.HasInputAuthority)
        {
            LocalPlayer.ShowPickable=true;
            PickeableUI.SetActive(LocalPlayer.ShowPickable);
            //IsPickeableUIActive = true;
            gunManager = FindFirstObjectByType<GunManager>();
            //gunManager = other.gameObject.GetComponentInChildren<GunManager>();
            if (gunManager != null)
            {
                gunManager.EnterPickeableGun(gunType);
                GunScriptableObject gun = gunManager.GetGun(gunType); //Pa tomar lo que quiran del arma a pickear
                damageText.text = $"DMG: {gun.Damage}";
                fireRateText.text = $"Fire Rate: {gun.ShootConfig.FireRate}";
                magazineText.text = $"Magazine: {gun.MagazineSize}";
            }


            lookCamera = PickeableUI.GetComponentInChildren<ObjectLookAtCamera>();
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
            //IsPickeableUIActive = false;
            LocalPlayer.ShowPickable = false;
            PickeableUI.SetActive(LocalPlayer.ShowPickable);
        }
    }
    private void UpdatePickeableUIState()
    {
        LocalPlayer.RPC_ActivateObject(IsPickeableUIActive);
        PickeableUI.SetActive(LocalPlayer.ShowPickable);
       // PickeableUI.SetActive(IsPickeableUIActive);
    }
}
