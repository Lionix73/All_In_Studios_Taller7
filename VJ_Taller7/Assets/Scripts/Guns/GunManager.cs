using System.Collections;
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
    public TextMeshProUGUI ammunitionDisplay; //UI de la municion que le queda en el cargador

    [SerializeField] private List<GunScriptableObject> gunsList;
    [SerializeField] private Transform gunParent;
    [SerializeField] private GunType Gun; //Tipo de arma que tiene el jugador

    public Transform aimRigPoint;  //La verdadera dirección de apuntado, coincide con el punto central de la camara.
    [SerializeField] private bool inAPickeableGun;

    private bool shooting;

    [Space]
    public GunScriptableObject CurrentGun;
    [SerializeField] private GunType CurrentSecondGunType;
    private int CurrentSecondaryGunBulletsLeft;

    public GunScriptableObject ActiveBaseGun { get; private set; }
    private GunType gunToPick;

    private void Awake() {
        actualTotalAmmo=MaxTotalAmmo;
        gunParent = this.transform;

        GunScriptableObject gun = gunsList.Find(gun => gun.Type == Gun);
        if (gun == null) {
            Debug.LogError($"No se ha encontrado el arma: {CurrentGun}");
            return;
        }
        SetUpGun(gun);
        CurrentSecondaryGunBulletsLeft = CurrentGun.MagazineSize;
        CurrentSecondGunType=Gun;
        inAPickeableGun=false;
    }

    private void Update() {
        if (totalAmmoDisplay != null) {
            totalAmmoDisplay.SetText(actualTotalAmmo + "/" + MaxTotalAmmo);
            if (UIManager.Singleton !=null)
            {
                UIManager.Singleton.GetPlayerTotalAmmo(actualTotalAmmo);
            }
            
        }
        if (shooting && CurrentGun.BulletsLeft > 0) {
            CurrentGun.Shoot();
        }
        else if (CurrentGun.BulletsLeft<=0 && !CurrentGun.Realoading){
            RealoadGun();
        }


        if (ammunitionDisplay != null) {
            ammunitionDisplay.SetText(CurrentGun.BulletsLeft + "/" + CurrentGun.MagazineSize);
            if (UIManager.Singleton != null)
            {
                UIManager.Singleton.GetPlayerActualAmmo(CurrentGun.BulletsLeft, CurrentGun.MagazineSize);
            }
        }

        if (actualTotalAmmo>MaxTotalAmmo){
            actualTotalAmmo=MaxTotalAmmo;
        }
    }

    private void SetUpGun(GunScriptableObject gun){
        ActiveBaseGun = gun;
        CurrentGun = gun.Clone() as GunScriptableObject;
        CurrentGun.Spawn(gunParent, this);
        if (UIManager.Singleton != null)
        {
        UIManager.Singleton.GetPlayerGunInfo(actualTotalAmmo, MaxTotalAmmo, gun);
        }
    }

    public void DespawnActiveGun(){
        if (CurrentGun!=null){
            CurrentGun.DeSpawn();
        }
        Destroy(CurrentGun);
    }

    public void OnShoot(InputAction.CallbackContext context) { //RECORDAR ASIGNAR MANUALMENTE EN LOS EVENTOS DEL INPUT
        if (CurrentGun.ShootConfig.IsAutomatic) {
            shooting = context.performed;
        }
        else { shooting = context.started; }
        //Debug.Log("Fase: " + shooting);
    }

    public void OnWeaponChange(InputAction.CallbackContext context){
        if (context.started){
            if (CurrentSecondGunType!=CurrentGun.Type){
                ChangeWeapon();
            }
        }
    }

    public void ChangeWeapon(){
        DespawnActiveGun();
        GunType temp = CurrentGun.Type;
        int tempAmmo = CurrentSecondaryGunBulletsLeft;
        CurrentSecondaryGunBulletsLeft = CurrentGun.BulletsLeft;
        GunScriptableObject gun = gunsList.Find(gun => gun.Type == CurrentSecondGunType);
        SetUpGun(gun);
        CurrentSecondGunType = temp;
        CurrentGun.BulletsLeft = tempAmmo;
    }

    public void GrabGun(GunType gunPicked){
        if (CurrentSecondGunType != gunPicked){
            if (CurrentSecondGunType == CurrentGun.Type){
                CurrentSecondGunType = CurrentGun.Type;
            }
            DespawnActiveGun();
            this.Gun = gunPicked;
            GunScriptableObject gun = gunsList.Find(gun => gun.Type == gunPicked);
            SetUpGun(gun);
        }
    }
    public void OnGrabGun(InputAction.CallbackContext context){
        if (context.started){
            if (inAPickeableGun){
                GrabGun(gunToPick);
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Pickeable")){
            inAPickeableGun=true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Pickeable")){
            inAPickeableGun=false;
        }
    }

    public void EnterPickeableGun(GunType gunType){
        gunToPick = gunType;
    }

    public void OnReload(InputAction.CallbackContext context){
        if (context.started){
            if (!CurrentGun.Realoading){
                RealoadGun();
            }
        }
    }
    private void RealoadGun(){
        CurrentGun.Reload();
        actualTotalAmmo -= CurrentGun.MagazineSize - CurrentGun.BulletsLeft;
    }
}
