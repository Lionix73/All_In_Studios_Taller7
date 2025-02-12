using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Cinemachine;

public class GunManager : MonoBehaviour
{
    public int actualTotalAmmo; //Cuanta municion tiene el jugador
    [SerializeField] private int MaxTotalAmmo; //Cuanta municion puede llevar el jugador

    public TextMeshProUGUI totalAmmoDisplay; //UI de la municion total que le queda al jugador

    [SerializeField] private List<GunScriptableObject> gunsList;
    [SerializeField] private Transform gunParent;
    [SerializeField] private GunType Gun;

    public CinemachineBrain cinemachineBrain;
    public Camera playerCamera;
    public Transform aimRigPoint;

    private bool shooting;

    [Space]
    public GunScriptableObject CurrentGun;

    private void Awake() {
        actualTotalAmmo=MaxTotalAmmo;
        gunParent = this.transform;
        ActiveWeapon();
        if (cinemachineBrain!=null){
            playerCamera = cinemachineBrain.GetComponent<Camera>();
        }
        
    }

    private void Update() {
        if (totalAmmoDisplay != null) {
            totalAmmoDisplay.SetText(actualTotalAmmo + "/" + MaxTotalAmmo);
        }
        if (shooting) {
            CurrentGun.Shoot();
        }
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit aimHit;

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out aimHit)) {
            targetPoint = aimHit.point;
        }
        else { targetPoint = ray.GetPoint(CurrentGun.TrailConfig.MissDistance); }

        Vector3 aimDirection = cinemachineBrain.gameObject.transform.position-targetPoint;

        //aimRigPoint.transform.position = aimDirection;
    }

    public void ActiveWeapon(){
        GunScriptableObject gun = gunsList.Find(gun => gun.Type == Gun);
        if (gun == null) {
            Debug.LogError($"No se ha encontrado el arma: {CurrentGun}");
            return;
        }

        CurrentGun = gun;
        CurrentGun.Spawn(gunParent, this);
    }

    public void OnShoot(InputAction.CallbackContext context) { //RECORDAR ASIGNAR MANUALMENTE EN LOS EVENTOS DEL INPUT
        if (CurrentGun.ShootConfig.IsAutomatic) {
            shooting = context.performed;
        }
        else { shooting = context.started; }
        //Debug.Log("Fase: " + shooting);
    }
}
