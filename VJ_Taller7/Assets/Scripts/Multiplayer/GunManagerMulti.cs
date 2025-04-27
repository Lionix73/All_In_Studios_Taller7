using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Cinemachine;
using System.Runtime.InteropServices;
using Unity.Netcode;
using UnityEngine.Pool;
using Unity.VisualScripting;
using UnityEngine.Animations;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;

//[RequireComponent(typeof(CrosshairManager))]
public class GunManagerMulti : NetworkBehaviour
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
    [SerializeField] private List<GunScriptableObject> gunsList;
    // Variable para sincronizar el NetworkObjectId
    private NetworkVariable<ulong> syncedObjectId = new NetworkVariable<ulong>();
    private NetworkObject parentNetworkObject;
    private NetworkObject modelNetworkObject;
    [SerializeField] public NetworkObject GunParent { get; set; }
    [SerializeField] public ParentConstraint GunHold { get; set; }
    [Tooltip("Tipo de arma que posee el jugador, define con cuál empieza")]
    [SerializeField] public GunType Gun; //Tipo de arma que tiene el jugador
    private Transform secondHandGrabPoint; // la posicion a asignar
    private Transform secondHandRigTarget; //el Rig en sí

    [SerializeField] private bool inAPickeableGun;
    private GunType gunToPick;

    private bool shooting;

    [Space]
    [Header("Active Guns Info")]

    public GunScriptableObject CurrentGun;
    [SerializeField] private GunType CurrentSecondGunType;
    private int CurrentSecondaryGunBulletsLeft;
    public bool IsLocalPlayer;



    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        cinemachineBrain = GameObject.Find("CinemachineBrain").GetComponent<CinemachineBrain>();

        Camera = cinemachineBrain.GetComponent<Camera>();
        actualTotalAmmo = MaxTotalAmmo;
        inAPickeableGun = false;
        
        SetUpGunParentRpc();//En caso de no tener asignado el punto de la mano donde aparece el arma, que la sostenga encima

        if(IsServer)
        {
            GunScriptableObject gun = gunsList.Find(gun => gun.Type == Gun);
            if (gun == null)
            {
                Debug.LogError($"No se ha encontrado el arma: {CurrentGun}");
                return;
            }
            SetUpGunRpc(Gun);
        }
    }

    [Rpc(SendTo.Server)]
    public void SetUpGunParentRpc()
    {
    }

private void Update() {
        /*if (!IsLocalPlayer || GunHold == null) return;
        transform.position = GunHold.position;
        transform.rotation = GunHold.rotation;*/

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
        if (UIManager.Singleton != null && IsOwner)
        {
            UIManager.Singleton.GetPlayerGunInfo(CurrentGun.BulletsLeft, CurrentGun.MagazineSize, CurrentGun);
        }

        if (ammunitionDisplay != null) {
            ammunitionDisplay.SetText(CurrentGun.BulletsLeft + "/" + CurrentGun.MagazineSize);
        }


        if (actualTotalAmmo>MaxTotalAmmo){
            actualTotalAmmo=MaxTotalAmmo;
        }
    }
    [Rpc(SendTo.Everyone)]
    public void SetUpGunRpc(GunType gunType){
        // Busca el GunScriptableObject en la lista de armas
        GunScriptableObject gun = gunsList.Find(g => g.Type == gunType);
        if (gun == null)
        {
            Debug.LogError($"No se encontró el arma con tipo: {gunType}");
            return;
        }
        CurrentGun = gun.Clone() as GunScriptableObject;
        CurrentSecondaryGunBulletsLeft = CurrentGun.MagazineSize;
        CurrentSecondGunType = Gun;
        SetUpGunRigs();
    }

    [Rpc(SendTo.Everyone)]
    public void GetCurrentGunRpc(GunType gunType)
    {
        // Busca el GunScriptableObject en la lista de armas
        GunScriptableObject gun = gunsList.Find(g => g.Type == gunType);
        if (gun == null)
        {
            Debug.LogError($"No se encontró el arma con tipo: {gunType}");
            return;
        }
        //gun.ActiveMonoBehaviour = ActiveMonoBehaviour;
        CurrentGun.LastShootTime = 0f;
        if (CurrentGun.bulletsLeft == 0)
        { //En revision porque no se recarga al recoger el arma, ni la primera vez que aparece.
            CurrentGun.bulletsLeft = CurrentGun.MagazineSize;
        }
        GetCurrentGunRpc(gunType);

        CurrentGun.realoading = false;
        CurrentGun.TrailPool = new ObjectPool<TrailRenderer>(gun.CreateTrail);
        SpawnRpc(gunType);

    }
    [Rpc(SendTo.Server)]
    public void SpawnRpc(GunType gunType)
    {

        // Busca el GunScriptableObject en la lista de armas
        GunScriptableObject gun = gunsList.Find(g => g.Type == gunType);
        if (gun == null)
        {
            Debug.LogError($"No se encontró el arma con tipo: {gunType}");
            return;
        }


        CurrentGun.Model = Instantiate(gun.ModelPrefab);
        modelNetworkObject = CurrentGun.Model.GetComponent<NetworkObject>();
        if (modelNetworkObject != null)
        {
            // Spawnea el modelo
            modelNetworkObject.Spawn();

            // Obtén el NetworkObject del padre (gunParent)
            
            SetParentGunRpc(modelNetworkObject.NetworkObjectId);

        }
        else
        {
            Debug.LogError("El modelo no tiene un NetworkObject.");
        }

        //modelNetworkObject.transform.localPosition = gun.SpawnPoint;
        //modelNetworkObject.transform.localEulerAngles = gun.SpawnRotation;

        //gun.activeCamera = camera;

    }

    [Rpc(SendTo.Everyone)]
    public void SetParentGunRpc(ulong modelNetworkObjectId)
    {

        // Obtén el NetworkObject correspondiente al ID
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(modelNetworkObjectId, out NetworkObject instanceGunManNet))
        {
            // Asigna el padre
            bool success = instanceGunManNet.TrySetParent(transform, false);
            if (success)
            {
                Debug.Log("Parent set successfully!");
            }
            else
            {
                Debug.LogError("Failed to set parent.");
            }

        }
        else
        {
            Debug.LogError("Failed to find NetworkObject with ID: " + modelNetworkObjectId);
        }
        CurrentGun.Model = instanceGunManNet.GameObject();
        CurrentGun.ShootSystem = CurrentGun.Model.GetComponentInChildren<ParticleSystem>();

    }
    [Rpc(SendTo.Server)]
    public void DeSpawnRpc(GunType gunType)
    {
        //Destroy(Model);
        /*gun.Model.SetActive(false);
        gun.GetComponent<NetworkObject>().Despawn();
        Destroy(gun.Model);
        gun.TrailPool.Clear();
        if (gun.BulletPool != null)
        {
            gun.BulletPool.Clear();
        }*/
    }

    public void DespawnActiveGun(){
        if (CurrentGun!=null){
            //DeSpawnRpc(CurrentGun);
        }
        Destroy(CurrentGun);
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
        SetUpGunRpc(CurrentSecondGunType);
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
            SetUpGunRpc(gunPicked);
        }
    }
    public void OnGrabGun(InputAction.CallbackContext context){
        if (context.started){
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
        if (context.started){
            if (!CurrentGun.Realoading){
                RealoadGun();
            }
        }
    }
    private void RealoadGun(){
        CurrentGun.Reload();
        actualTotalAmmo -= CurrentGun.MagazineSize - CurrentGun.BulletsLeft;
        if (UIManager.Singleton != null)
        {
            UIManager.Singleton.GetPlayerActualAmmo(CurrentGun.BulletsLeft, CurrentGun.MagazineSize);
        }
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
