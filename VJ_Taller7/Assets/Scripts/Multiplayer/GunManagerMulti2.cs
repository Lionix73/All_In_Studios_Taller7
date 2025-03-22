using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.Pool;
using TMPro;
using Unity.Cinemachine;
using System.Runtime.InteropServices;
using UnityEngine.Animations;
using Unity.VisualScripting;
using System;

//[RequireComponent(typeof(CrosshairManager))]
public class GunManagerMulti2 : NetworkBehaviour
{
    MonoBehaviour m_MonoBehaviour;
    [Header("Camera")]
    public Camera camera; public CinemachineBrain cinemachineBrain;  
    [Header("Ammo Info")]
    private NetworkVariable<int> actualTotalAmmo = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);
    //public int actualTotalAmmo; //Cuanta municion tiene el jugador
    [SerializeField] private int MaxTotalAmmo; //Cuanta municion puede llevar el jugador

    public TextMeshProUGUI totalAmmoDisplay; //UI de la municion total que le queda al jugador
    public TextMeshProUGUI ammunitionDisplay; //UI de la municion que le queda en el cargador

    [Header("Gun General Info")]
    [Tooltip("Lista de las armas que existen en el juego")]
    private WeaponLogic weapon;
    [SerializeField] private List<GunScriptableObject> gunsList;
    [SerializeField] private Transform gunParent;
    [SerializeField] private Transform gunRig;
    [Tooltip("Tipo de arma que posee el jugador, define con cuál empieza")]
    [SerializeField] public GunType Gun; //Tipo de arma que tiene el jugador
    private NetworkVariable<GunType> GunNet = new NetworkVariable<GunType>(0, NetworkVariableReadPermission.Everyone);
    private Transform secondHandGrabPoint; // la posicion a asignar
    private Transform secondHandRigTarget; //el Rig en sí

    [SerializeField] private bool inAPickeableGun;
    private GunType gunToPick;

    private bool shooting;

    [Space]
    [Header("Active Guns Info")]

    public GunScriptableObject CurrentGun;
    [SerializeField] private GunType CurrentSecondGunType;
    private NetworkVariable<GunType> CurrentSecondGunTypeNet = new NetworkVariable<GunType>(0, NetworkVariableReadPermission.Everyone);
    private NetworkVariable<int> CurrentSecondaryGunBulletsLeft = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);
    //private int CurrentSecondaryGunBulletsLeft;




    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        GunScriptableObject gun = gunsList.Find(gun => gun.Type == Gun);
        if (gun == null)
        {
            Debug.LogError($"No se ha encontrado el arma: {CurrentGun}");
            return;
        }
        CurrentGun = gun.Clone() as GunScriptableObject;
        //cinemachineBrain = GameObject.Find("CinemachineBrain").GetComponent<CinemachineBrain>();


        SendActualAmmoRpc(MaxTotalAmmo);
        if (!IsLocalPlayer) return;

            SpawnGunRpc(Gun);
            camera = Camera.main;

            ChangeGunTypeRpc(Gun);
            //SetUpGunRpc(GunNet.Value);
            SendSecondaryGunBulletsLeftRpc(CurrentGun.MagazineSize);
            ChangeSecondGunTypeRpc(Gun);
            //CurrentSecondGunType = Gun;
            ChangeSecondGunTypeRpc(CurrentSecondGunTypeNet.Value);
            inAPickeableGun = false;

            secondHandRigTarget = GameObject.Find("SecondHandGripRig_target").GetComponent<Transform>();

    }

    private void Update() {
        if (!IsOwner) return;
        if (totalAmmoDisplay != null) {
            totalAmmoDisplay.SetText(actualTotalAmmo + "/" + MaxTotalAmmo);
            
        }
        if (UIManager.Singleton !=null)
        {
            UIManager.Singleton.GetPlayerTotalAmmo(actualTotalAmmo.Value);
        }
        if (shooting && weapon.BulletsLeft > 0) {
            weapon.Shoot();
        }
        else if (CurrentGun.bulletsLeft<=0 && !CurrentGun.realoading){
            RealoadGun();
        }


        if (ammunitionDisplay != null) {
            ammunitionDisplay.SetText(CurrentGun.BulletsLeft + "/" + CurrentGun.MagazineSize);
        }
        if (UIManager.Singleton != null)
        {
            UIManager.Singleton.GetPlayerActualAmmo(CurrentGun.BulletsLeft, CurrentGun.MagazineSize);
        }

        if (actualTotalAmmo.Value >MaxTotalAmmo){
            actualTotalAmmo.Value =MaxTotalAmmo;
        }
    }
    [Rpc(SendTo.Everyone)]
    private void SetUpGunRpc(GunType gunType){
        if (gunParent == null) { gunParent = GetComponentInParent<NetworkObject>().transform; } //En caso de no tener asignado el punto de la mano donde aparece el arma, que la sostenga encima

        GunScriptableObject gun = gunsList.Find(gun => gun.Type == gunType);
        if (gun == null)
        {
            Debug.LogError($"No se ha encontrado el arma: {CurrentGun}");
            return;
        }
        //CurrentGun.ActiveMonoBehaviour = this;
        CurrentGun = gun.Clone() as GunScriptableObject;
        CurrentGun.LastShootTime = 0f;
        if (CurrentGun.bulletsLeft == 0)
        { //En revision porque no se recarga al recoger el arma, ni la primera vez que aparece.
            CurrentGun.bulletsLeft = CurrentGun.MagazineSize;
        }
        CurrentGun.realoading = false;
        CurrentGun.TrailPool = new ObjectPool<TrailRenderer>(gun.CreateTrail);
        if(IsServer)
        {
            SpawnGunRpc(gunType);
        }




        if (UIManager.Singleton != null)
        {
            UIManager.Singleton.GetPlayerGunInfo(CurrentGun.BulletsLeft, CurrentGun.MagazineSize, CurrentGun);
        }
        if(IsOwner)
        {
            SetUpGunRigs();
        }
    }
    [Rpc(SendTo.Server)]
    public void SpawnGunRpc(GunType gunType)
    {
        Debug.Log("spawn Arma");
        CurrentGun.Model = Instantiate(CurrentGun.ModelPrefab);
        NetworkObject modelNetworkObject = CurrentGun.Model.GetComponent<NetworkObject>();
        modelNetworkObject.Spawn();
        bool success = modelNetworkObject.TrySetParent(gunParent, false);
        if (success)
        {
            Debug.Log("Parent set successfully!");
        }
        else
        {
            Debug.LogError("Failed to set parent.");
        }
        SetParentGunRpc(modelNetworkObject.NetworkObjectId);
    }
    [Rpc(SendTo.Server)]
    void ChangeGunTypeRpc(GunType value)
    {
        GunNet.Value = value;
    }
    [Rpc(SendTo.Server)]
    void ChangeSecondGunTypeRpc(GunType value)
    {
        GunNet.Value = value;
    }
    [Rpc(SendTo.Everyone)]
    public void SetParentGunRpc(ulong modelNetworkObjectId)
    {

        // Obtén el NetworkObject correspondiente al ID
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(modelNetworkObjectId, out NetworkObject spawnGun))
        {
            // Asigna el padre
            CurrentGun.Model = spawnGun.gameObject;
            spawnGun.GetComponent<FollowTransform>().SetTargetTransform(transform);
        }
        else
        {
            Debug.LogError("Failed to find NetworkObject with ID: " + modelNetworkObjectId);
        }
        CurrentGun.ShootSystem = CurrentGun.Model.GetComponentInChildren<ParticleSystem>();
        weapon = spawnGun.gameObject.GetComponent<WeaponLogic>();
        gunRig = CurrentGun.Model.transform;

    }
    [Rpc(SendTo.Server)]
    public void SendActualAmmoRpc(int actualAmmo)
    {
        actualTotalAmmo.Value = actualAmmo;
    }
    [Rpc(SendTo.Server)]
    public void SendSecondaryGunBulletsLeftRpc(int bulletsLeft)
    {
        CurrentSecondaryGunBulletsLeft.Value = bulletsLeft;
    }
    [Rpc(SendTo.Everyone)]
    public void DespawnActiveGunRpc(){
        if (CurrentGun!=null){
            DespawnRpc();
        }
        //CurrentGun.TrailPool.Clear();
        //if (CurrentGun.BulletPool != null)
        //{
          //  CurrentGun.BulletPool.Clear();
        //}
        Destroy(CurrentGun);
    }
    [Rpc(SendTo.Server)]
    public void DespawnRpc()
    {
        CurrentGun.Model.SetActive(false);
        CurrentGun.Model.GetComponent<NetworkObject>().Despawn();
        Destroy(CurrentGun.Model);

    }

    private void SetUpGunRigs(){
        Transform[] chGun = gunRig.GetComponentsInChildren<Transform>();
        //secondHandGrabPoint = GameObject.Find("SecondHanGrip").transform;
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
        if (!IsOwner) return;
        if (context.started && weapon.ShootConfig.IsAutomatic)
        {
            //weapon.Shoot();
            shooting = true;
        }
        else if (!weapon.ShootConfig.IsAutomatic)
        {
            //weapon.Shoot();
            shooting = context.started;
        } 
        
        if (context.canceled && weapon.ShootConfig.IsAutomatic)
        {
            shooting = false; 
        }
        //Debug.Log("Fase: " + shooting);
    }

    public void OnWeaponChange(InputAction.CallbackContext context){
        if (!IsOwner) return;
        if (context.started){
            if (CurrentSecondGunType!=CurrentGun.Type){
                ChangeWeapon();
            }
        }
    }

    public void ChangeWeapon(){
        DespawnRpc();
        GunType temp = CurrentGun.Type;
        //int tempAmmo = CurrentSecondaryGunBulletsLeft.Value;
        //CurrentSecondaryGunBulletsLeft.Value = CurrentGun.BulletsLeft;
        GunScriptableObject gun = gunsList.Find(gun => gun.Type == CurrentSecondGunType);

        SetUpGunRpc(CurrentSecondGunType);
        ChangeGunTypeRpc(CurrentSecondGunType);
        Gun = CurrentSecondGunType;
        CurrentSecondGunType = temp;
        ChangeSecondGunTypeRpc(CurrentSecondGunType);
        //CurrentGun.BulletsLeft = tempAmmo;
    }

    public void GrabGun(GunType gunPicked){
        if (CurrentSecondGunType != gunPicked){
            if (CurrentSecondGunType == CurrentGun.Type){
                CurrentSecondGunType = CurrentGun.Type;
            }
            DespawnRpc();
            Gun = gunPicked;
            GunScriptableObject gun = gunsList.Find(gun => gun.Type == gunPicked);
            SetUpGunRpc(gunPicked);
        }
    }
    public void OnGrabGun(InputAction.CallbackContext context){
        if (!IsOwner) return;
        if (context.started){
            Debug.Log("Cogiendo Arma");
            if (inAPickeableGun){
                GrabGun(gunToPick);
            }
        }
    }

    #region Pickeables management //Aqui sabemos si el jugador esta cerca de un arma, que tipo de arma es y si puede recogerla

    public void EnterPickeableGun(GunType gunType){
        gunToPick = gunType;
        inAPickeableGun = true;
    }

    public void ExitPickeableGun(){
        inAPickeableGun = false;
    }
    #endregion

    #region Ammo management //Aqui recibimos el input de recarga y llamamos a la funciond de recarga
    public void OnReload(InputAction.CallbackContext context){
        if (!IsOwner) { return; }
        if (context.started){
            if (!CurrentGun.Realoading){
                RealoadGun();
            }
        }
    }
    private void RealoadGun(){
        if (IsOwner) return;
        CurrentGun.Reload();
        actualTotalAmmo.Value -= CurrentGun.MagazineSize - CurrentGun.BulletsLeft;
    }
    #endregion

/// <summary>
/// Devuelve el arma que se le pida
/// </summary>
/// <param name="gunToFind">Tipo del arma que se quiere encontrar.</param>
/// <returns></returns>
    public GunScriptableObject GetGun(GunType gunToFind){
        return gunsList.Find(gun => gun.Type == gunToFind);
    }
}
