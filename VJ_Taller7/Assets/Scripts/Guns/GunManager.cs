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
    public GunScriptableObject CurrentSecondaryGun;
    private GunType gunToPick;

    private void Awake() {
        actualTotalAmmo=MaxTotalAmmo;
        gunParent = this.transform;
        ActiveWeapon();
        CurrentSecondaryGun=null;
        inAPickeableGun=false;
        
    }

    private void Update() {
        if (totalAmmoDisplay != null) {
            totalAmmoDisplay.SetText(actualTotalAmmo + "/" + MaxTotalAmmo);
        }
        if (shooting && CurrentGun.BulletsLeft > 0) {
            CurrentGun.Shoot();
        }
        else if (CurrentGun.BulletsLeft<=0 && !CurrentGun.Realoading){
            RealoadGun();
        }


        if (ammunitionDisplay != null) {
            ammunitionDisplay.SetText(CurrentGun.BulletsLeft + "/" + CurrentGun.MagazineSize);
        }

        if (actualTotalAmmo>MaxTotalAmmo){
            actualTotalAmmo=MaxTotalAmmo;
        }
    }

    public void ActiveWeapon(){
        GunScriptableObject gun = gunsList.Find(gun => gun.Type == Gun);
        if (gun == null) {
            Debug.LogError($"No se ha encontrado el arma: {CurrentGun}");
            return;
        }

        CurrentGun = gun;
        CurrentGun.Spawn(gunParent, this);
        if (CurrentSecondaryGun!=null){
            CurrentSecondaryGun.DeSpawn();
        }
        
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
            if (CurrentSecondaryGun!=null){
                ChangeWeapon();
            }
        }
    }

    public void ChangeWeapon(){
        if (Gun == CurrentGun.Type) {
            GunType temp = Gun;
            Gun = CurrentSecondaryGun.Type;
            CurrentSecondaryGun= gunsList.Find(gun => gun.Type == temp);
        }
        ActiveWeapon();
    }

    public void GrabGun(GunType gunPicked){
        if (CurrentGun.Type!=gunPicked){
            CurrentSecondaryGun = CurrentGun;
            CurrentSecondaryGun = CurrentGun;
            Gun = gunPicked;
            ActiveWeapon();
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
