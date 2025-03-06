using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Cinemachine;
using System.Runtime.InteropServices;
using Fusion;

public class GunManagerPhoton : NetworkBehaviour
{
    [Header("Camera")]
    public Camera Camera; public CinemachineBrain cinemachineBrain;  
    [Header("Ammo Info")]
    public int actualTotalAmmo; //Cuanta municion tiene el jugador
    [SerializeField] private int MaxTotalAmmo; //Cuanta municion puede llevar el jugador

    public TextMeshProUGUI totalAmmoDisplay; //UI de la municion total que le queda al jugador
    public TextMeshProUGUI ammunitionDisplay; //UI de la municion que le queda en el cargador

    [Header("Gun General Info")]
    [Tooltip("Lista de las armas que existen en el juego")]
    [SerializeField] private List<GunScriptableObjectPhoton> gunsList;
    [Networked] NetworkTransform gunParent { get; set; }
    [Tooltip("Tipo de arma que posee el jugador, define con cuál empieza")]
    [Networked] public GunType Gun { get; private set; } //Tipo de arma que tiene el jugador
    private Transform secondHandGrabPoint; // la posicion a asignar
    private Transform secondHandRigTarget; //el Rig en sí

    [Networked] public bool inAPickeableGun {  get; private set; }
    private GunType gunToPick;

    private bool shooting;

    [Space]
    [Header("Active Guns Info")]

    GunScriptableObjectPhoton CurrentGun;
    [SerializeField] private GunType CurrentSecondGunType;
    private int CurrentSecondaryGunBulletsLeft;

    TickTimer ReloadDuration;

    [Networked] public PlayerControllerPhoton LocalPlayer {  get; private set; }
    [Networked] public NetworkObject Model {  get; set; }

    public override void Spawned() {

        cinemachineBrain = GameObject.Find("CinemachineBrain").GetComponent<CinemachineBrain>();
        Camera = cinemachineBrain.GetComponent<Camera>();
        actualTotalAmmo = MaxTotalAmmo;
        gunParent = GetComponent<NetworkTransform>();

        SetUpGun();
        CurrentSecondaryGunBulletsLeft = CurrentGun.MagazineSize;
        CurrentSecondGunType=Gun;
        inAPickeableGun=false;
    }

    /*private void Update() {
        if (totalAmmoDisplay != null) {
            totalAmmoDisplay.SetText(actualTotalAmmo + "/" + MaxTotalAmmo);
            
        }
        if (UIManager.Singleton !=null)
        {
            UIManager.Singleton.GetPlayerTotalAmmo(actualTotalAmmo);
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
        if (UIManager.Singleton != null)
        {
            UIManager.Singleton.GetPlayerActualAmmo(CurrentGun.BulletsLeft, CurrentGun.MagazineSize);
        }

        if (actualTotalAmmo>MaxTotalAmmo){
            actualTotalAmmo=MaxTotalAmmo;
        }
    }*/
    public void SetUpGun(){
        GunScriptableObjectPhoton gun = gunsList.Find(gun => gun.Type == Gun);
        if (gun == null)
        {
            Debug.LogError($"No se ha encontrado el arma: {CurrentGun}");
            return;
        }
        gunParent = GetComponent<NetworkTransform>();
        CurrentGun = gun.Clone() as GunScriptableObjectPhoton;
        //CurrentGun.Spawn(gunParent);
       CurrentGun.LastShootTime = 0f;
        if (CurrentGun.BulletsLeft == 0)
            CurrentGun.BulletsLeft = CurrentGun.MagazineSize;
        
        CurrentGun.realoading = false;
        if (Runner.IsServer)
        {
            Model = Runner.Spawn(CurrentGun.ModelPrefab);
        }
        Model.transform.SetParent(gunParent.transform, false);
        Model.transform.localPosition = CurrentGun.SpawnPoint;
        Model.transform.localEulerAngles = CurrentGun.SpawnRotation;
       /* if (UIManager.Singleton != null)
        {
            UIManager.Singleton.GetPlayerGunInfo(CurrentGun.BulletsLeft, CurrentGun.MagazineSize, CurrentGun);
        }*/

        SetUpGunRigs();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_DespawnActiveGun(){
        if (CurrentGun!=null){
            //CurrentGun.Model.SetActive(false);
            if (Runner.IsServer) { 
            
                Runner.Despawn(Model);
                Model = null;
            }
        }
        //Destroy(CurrentGun);
    }

    private void SetUpGunRigs(){
        Transform[] chGun = GetComponentsInChildren<Transform>();
        for(int i = 0; i < chGun.Length; i++){
            if (chGun[i].name == "SecondHandGrip") {
                secondHandGrabPoint = chGun[i].transform;
            }
        }
        if (secondHandRigTarget==null) return;
        secondHandRigTarget.position = secondHandGrabPoint.position;
    }


    public void OnShoot(InputAction.CallbackContext context) { //RECORDAR ASIGNAR MANUALMENTE EN LOS EVENTOS DEL INPUT
        //if (CurrentGun.ShootConfig.IsAutomatic) {
        //    shooting = context.performed;
        //}
        //else { shooting = context.started; }

        if (context.started && CurrentGun.ShootConfig.IsAutomatic) 
            shooting=true;
        else if (!CurrentGun.ShootConfig.IsAutomatic) shooting = context.started;
        if (context.canceled && CurrentGun.ShootConfig.IsAutomatic)
            shooting = false; 
        //Debug.Log("Fase: " + shooting);
    }

    public void OnWeaponChange(){
        if (CurrentSecondGunType != CurrentGun.Type)
        {
            RPC_ChangeWeapon();
            Debug.Log("CambiandoArma");
        }
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ChangeWeapon(){
        if (CurrentSecondGunType != CurrentGun.Type)
        {
            RPC_DespawnActiveGun();
            gunParent =GetComponent<NetworkTransform>();
            GunType temp = CurrentGun.Type;
            int tempAmmo = CurrentSecondaryGunBulletsLeft;
            CurrentSecondaryGunBulletsLeft = CurrentGun.BulletsLeft;
            GunScriptableObjectPhoton gun = gunsList.Find(gun => gun.Type == CurrentSecondGunType);
            CurrentGun = gun.Clone() as GunScriptableObjectPhoton;
            //CurrentGun.Spawn(gunParent);
            CurrentGun.LastShootTime = 0f;
            if (CurrentGun.BulletsLeft == 0)
                CurrentGun.BulletsLeft = CurrentGun.MagazineSize;

            CurrentGun.realoading = false;
            
            Model = Runner.Spawn(CurrentGun.ModelPrefab, null, null, Object.InputAuthority);
            
            CurrentSecondGunType = temp;
            CurrentGun.BulletsLeft = tempAmmo;
        }
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_GrabGun(GunType gunPicked){
        if (CurrentSecondGunType != gunPicked){
            if (CurrentSecondGunType == CurrentGun.Type){
                CurrentSecondGunType = CurrentGun.Type;
            }
            gunParent = GetComponent<NetworkTransform>();
            RPC_DespawnActiveGun();
            this.Gun = gunPicked;
            GunScriptableObjectPhoton gun = gunsList.Find(gun => gun.Type == gunPicked);
            CurrentGun = gun.Clone() as GunScriptableObjectPhoton;
            //CurrentGun.Spawn(gunParent);
            CurrentGun.LastShootTime = 0f;
            if (CurrentGun.BulletsLeft == 0)
                CurrentGun.BulletsLeft = CurrentGun.MagazineSize;

            CurrentGun.realoading = false;
           
            Model = Runner.Spawn(CurrentGun.ModelPrefab, null, null, Object.InputAuthority);

        }
    }
    [ContextMenu("RCP_OganizeWeapon")]
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_OrganizeWeapon()
    {

        Model.transform.SetParent(gunParent.transform, false);
        Debug.Log(gunParent.transform);
        Model.transform.localPosition = CurrentGun.SpawnPoint;
        Model.transform.localEulerAngles = CurrentGun.SpawnRotation;
    }
    public void OnGrabGun(){
            if (inAPickeableGun){
                RPC_GrabGun(gunToPick);
                
            }
    }

    #region Pickeables management //Aqui sabemos si el jugador esta cerca de un arma, que tipo de arma es y si puede recogerla
    
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Pickeable"))
        {
            if (other.TryGetComponent<GunPickeablePhoton>(out GunPickeablePhoton component)){ inAPickeableGun=true; Debug.Log(inAPickeableGun); }
            
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Pickeable")){
            if (other.TryGetComponent<GunPickeablePhoton>(out GunPickeablePhoton component)){ inAPickeableGun=false;}
        }
    }

    public void EnterPickeableGun(GunType gunType){
        gunToPick = gunType;
    }
    #endregion

    #region Ammo management //Aqui recibimos el input de recarga y llamamos a la funciond de recarga
    public void OnReload(InputAction.CallbackContext context){
        if (context.started){
            if (!CurrentGun.realoading){
                RealoadGun();
            }
        }
    }
    private void RealoadGun(){
        CurrentGun.Reload();
        actualTotalAmmo -= CurrentGun.MagazineSize - CurrentGun.BulletsLeft;
    }
    #endregion

/// <summary>
/// Devuelve el arma que se le pida
/// </summary>
/// <param name="gunToFind">Tipo del arma que se quiere encontrar.</param>
/// <returns></returns>
    public GunScriptableObjectPhoton GetGun(GunType gunToFind){
        return gunsList.Find(gun => gun.Type == gunToFind);
    }
}
