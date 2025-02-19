using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GunPickeable : MonoBehaviour
{
    [SerializeField] private GunType gunType;
    [SerializeField] private GameObject PickeableUI;
    [SerializeField] private TextMeshProUGUI gunName;
    [SerializeField] private ObjectLookAtCamera lookCamera;

    private Vector3 spinDirection = new Vector3(0, 1, 0);
    private GunManager gunManager;


    private void Awake() {
        //gunName.SetText(gunType.ToString() + "\n" + "Press E to pick up");
        gunName.SetText(gunType.ToString());
        PickeableUI.SetActive(false);
    }

    private void Update() {
        transform.Rotate(spinDirection);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            gunManager = other.gameObject.GetComponentInChildren<GunManager>();
            gunManager.EnterPickeableGun(gunType);

            PickeableUI.SetActive(true);
            lookCamera = PickeableUI.GetComponentInChildren<ObjectLookAtCamera>();

            GunScriptableObject gun = gunManager.GetGun(gunType); //Pa tomar lo que quiran del arma a pickear
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
}
