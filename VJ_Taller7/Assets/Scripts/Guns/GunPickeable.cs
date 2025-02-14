using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GunPickeable : MonoBehaviour
{
    [SerializeField] private GunType gunType;
    [SerializeField] private GameObject PickeableUI;
    [SerializeField] private TextMeshProUGUI gunName;
    private GunManager gunManager;

    private void Awake() {
        gunName.SetText(gunType.ToString() + "\n" + "Press E to pick up");
        PickeableUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            gunManager = other.gameObject.GetComponentInChildren<GunManager>();
            gunManager.EnterPickeableGun(gunType);

            PickeableUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            PickeableUI.SetActive(false);
        }
    }
}
