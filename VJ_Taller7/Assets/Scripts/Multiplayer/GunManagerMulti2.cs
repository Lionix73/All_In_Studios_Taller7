using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using TMPro;
using Unity.Cinemachine;
using System;



[RequireComponent(typeof(CrosshairManagerMulti))]
public class GunManagerMulti2 : NetworkBehaviour
{
    MonoBehaviour m_MonoBehaviour;
    [Header("Camera")]
    public Camera camera; public CinemachineBrain cinemachineBrain;
    private PlayerController player;
    [Header("Ammo Info")]
    [Header("Managers")]
    public CrosshairManagerMulti crosshairManager;
    private SoundManager soundManager; private Animator playerAnimator;
    private NetworkVariable<int> actualTotalAmmo = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);
    //public int actualTotalAmmo; //Cuanta municion tiene el jugador
    [SerializeField] private int MaxTotalAmmo; //Cuanta municion puede llevar el jugador

    public TextMeshProUGUI totalAmmoDisplay; //UI de la municion total que le queda al jugador
    public TextMeshProUGUI ammunitionDisplay; //UI de la municion que le queda en el cargador

    [Header("Gun General Info")]
    [Tooltip("Lista de las armas que existen en el juego")]
    [SerializeField] private int basePlayerDamage; //Con este se escala el game
    public WeaponLogic weapon;
    [SerializeField] private List<GunScriptableObject> gunsList;
    [SerializeField] private Transform gunParent;
    [SerializeField] private Transform gunRig;
    [Tooltip("Tipo de arma que posee el jugador, define con cuál empieza")]
    [SerializeField] public GunType Gun //Tipo de arma que tiene el jugador
    {
        get => GunNet.Value;
        set { if (IsServer) GunNet.Value = value; }
    }
    private NetworkVariable<GunType> GunNet = new NetworkVariable<GunType>(GunType.BasicPistol, NetworkVariableReadPermission.Everyone);
    private Transform secondHandGrabPoint; // la posicion a asignar
    [SerializeField] private Transform secondHandRigTarget; //el Rig en sí

    [SerializeField] private bool inAPickeableGun;
    private GunType gunToPick;
    private NetworkObject modelNetworkObject;

    private bool shooting;
    private bool canShoot = true;

    [Space]
    [Header("Active Guns Info")]

    public GunScriptableObject CurrentGun;
    [SerializeField] private GunType CurrentSecondGunType;
    private NetworkVariable<GunType> CurrentSecondGunTypeNet = new NetworkVariable<GunType>(0, NetworkVariableReadPermission.Everyone);
    private NetworkVariable<int> CurrentSecondaryGunBulletsLeft = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);
    private NetworkVariable<int> CurrentGunBulletsLeft = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);
    public event Action<int> OnBulletsChanged;

    private Vector3 dondePegaElRayDelArma;
    //private int CurrentSecondaryGunBulletsLeft;
    public delegate void ReloadingEvent(ulong ownerId);
    public event ReloadingEvent ReloadEvent;

    MultiPlayerState playerState;




    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer && IsClient && CurrentSecondGunTypeNet.Value == GunType.BasicPistol)
        {
            DespawnRpc();
            SetUpGunRpc(Gun);
            ChangeGunTypeRpc(Gun);
            ChangeSecondGunTypeRpc(Gun);
        }
        
        //cinemachineBrain = GameObject.Find("CinemachineBrain").GetComponent<CinemachineBrain>();
        if (!IsOwner) return;
        
        playerState = transform.root.GetComponentInChildren<MultiPlayerState>();
        SetUpGunRpc(Gun);
        SendActualAmmoRpc(MaxTotalAmmo);
        SendCurrentGunBulletsLeftRpc(CurrentGun.MagazineSize);
        
        
        //if (UIManager.Singleton && IsOwner) UIManager.Singleton.GetPlayerGunInfo(CurrentGunBulletsLeft.Value, weapon.MagazineSize, CurrentGun);
        //if (UIManager.Singleton && IsOwner) UIManager.Singleton.GetPlayerTotalAmmo(MaxTotalAmmo);
        camera = Camera.main;

        //secondHandRigTarget = GameObject.Find("SecondHandGripRig_target").GetComponent<Transform>();
        ChangeGunTypeRpc(Gun);
        //SetUpGunRpc(GunNet.Value);
        ChangeSecondGunTypeRpc(Gun);
        //CurrentSecondGunType = Gun;
        //ChangeSecondGunTypeRpc(CurrentSecondGunTypeNet.Value);
        inAPickeableGun = false;
        //ammunitionDisplay = GameObject.Find("AmmoGun").GetComponent<TextMeshProUGUI>();
        //totalAmmoDisplay = GameObject.Find("TotalAmmo").GetComponent<TextMeshProUGUI>();


    }
    public void RespawnGuns(GunType gun)
    {
        DespawnRpc();
        SetUpGunRpc(gun);
        SendActualAmmoRpc(MaxTotalAmmo);
        SendCurrentGunBulletsLeftRpc(CurrentGun.MagazineSize);
        ChangeGunTypeRpc(gun);
        ChangeSecondGunTypeRpc(gun);
    }

    private void Update() {
        if (!IsOwner || weapon == null) return;


        if (shooting && weapon.BulletsLeft > 0) {
            Debug.Log($"Disparando desde el cliente # {OwnerClientId}");
            weapon.Shoot(OwnerClientId);

        }
        /*else if (IsServer && weapon.BulletsLeft <= 0 && !weapon.realoading) {
            RealoadGun();
        }*/
        UIManager.Singleton.GetPlayerActualAmmo(weapon.BulletsLeft, weapon.MagazineSize);

        if (actualTotalAmmo.Value > MaxTotalAmmo) {
            SendActualAmmoRpc(MaxTotalAmmo);
        }
    }
    [Rpc(SendTo.Everyone)]
    private void SetUpGunRpc(GunType gunType) {
        if (gunParent == null) { gunParent = GetComponentInParent<NetworkObject>().transform; } //En caso de no tener asignado el punto de la mano donde aparece el arma, que la sostenga encima

        GunScriptableObject gun = gunsList.Find(gun => gun.Type == gunType);
        if (gun == null)
        {
            Debug.LogError($"No se ha encontrado el arma: {CurrentGun}");
            return;
        }
        //CurrentGun.ActiveMonoBehaviour = this;
        CurrentGun = gun.Clone() as GunScriptableObject;
        CurrentGun.Damage = basePlayerDamage;
        /* CurrentGun.LastShootTime = 0f;
         if (CurrentGun.bulletsLeft == 0)
         { //En revision porque no se recarga al recoger el arma, ni la primera vez que aparece.
             CurrentGun.bulletsLeft = CurrentGun.MagazineSize;
         }
         CurrentGun.realoading = false;
         CurrentGun.TrailPool = new ObjectPool<TrailRenderer>(gun.CreateTrail);*/
        if (IsServer)
        {
            SpawnGunRpc();
        }


        if (IsOwner)
        {
            SetUpGunRigsRpc();
        }
    }
    [Rpc(SendTo.Server)]
    public void SpawnGunRpc()
    {
        Debug.Log("spawn Arma");
        CurrentGun.Model = Instantiate(CurrentGun.NetworkModelPrefab);
        modelNetworkObject = CurrentGun.Model.GetComponent<NetworkObject>();

        modelNetworkObject.Spawn();
        Gun = CurrentGun.Type;
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
        Gun = value;
    }
    [Rpc(SendTo.Server)]
    void ChangeSecondGunTypeRpc(GunType value)
    {
        CurrentSecondGunTypeNet.Value = value;
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
        // CurrentGun.ShootSystem = CurrentGun.Model.GetComponentInChildren<ParticleSystem>();
        gunRig = CurrentGun.Model.transform;
        weapon = spawnGun.gameObject.GetComponent<WeaponLogic>();
        if (IsServer) weapon.OnEmptyAmmo += RealoadGun;
        if (UIManager.Singleton && IsOwner) UIManager.Singleton.GetPlayerGunInfo(CurrentGunBulletsLeft.Value, weapon.MagazineSize,CurrentGun);
        crosshairManager.SetCrosshairImage(CurrentGun.CrosshairImage);

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
    [Rpc(SendTo.Server)]
    public void SendCurrentGunBulletsLeftRpc(int bulletsLeft)
    {
        CurrentGunBulletsLeft.Value = bulletsLeft;
    }

    [Rpc(SendTo.Everyone)]
    public void DespawnActiveGunRpc() {
        if (CurrentGun != null) {
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
        weapon.OnEmptyAmmo -= RealoadGun;
        weapon.gameObject.SetActive(false);
        weapon.GetComponent<NetworkObject>().Despawn();
        Destroy(weapon);

    }

    [Rpc(SendTo.Everyone)]
    private void SetUpGunRigsRpc() {
        Transform[] chGun = gunRig.GetComponentsInChildren<Transform>();
        //secondHandGrabPoint = GameObject.Find("SecondHanGrip").transform;
        for (int i = 0; i < chGun.Length; i++) {
            if (chGun[i].name == "SecondHandGrip") {
                secondHandGrabPoint = chGun[i].transform;
            }
        }
        if (secondHandRigTarget == null) return;
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

    public void OnWeaponChange(InputAction.CallbackContext context) {
        if (!IsOwner) return;
        if (context.started) {
            if (CurrentSecondGunTypeNet.Value != GunNet.Value) {
                ChangeWeapon();
            }
        }
    }

    public void ChangeWeapon() {
        GunType tempType = CurrentGun.Type;
        int tempAmmo = weapon.BulletsLeft;

        ChangeWeaponRpc(tempType,tempAmmo);
        
        /*SendCurrentGunBulletsLeftRpc(CurrentSecondaryGunBulletsLeft.Value);
        SendSecondaryGunBulletsLeftRpc(weapon.bulletsLeft);
        DespawnRpc();
        //CurrentSecondaryGunBulletsLeft.Value = CurrentGun.BulletsLeft;
        GunScriptableObject gun = gunsList.Find(gun => gun.Type == CurrentSecondGunType);

        SetUpGunRpc(CurrentSecondGunType);
        ChangeGunTypeRpc(CurrentSecondGunType);
        Gun = CurrentSecondGunType;
        CurrentSecondGunType = temp;
        ChangeSecondGunTypeRpc(CurrentSecondGunType);*/
    }

    [Rpc(SendTo.Server)]
    private void ChangeWeaponRpc(GunType currentGunType,int currentAmmo)
    {
        CurrentGunBulletsLeft.Value = CurrentSecondaryGunBulletsLeft.Value;

        CurrentSecondaryGunBulletsLeft.Value = currentAmmo;

        StartCoroutine(SwitchWeaponCoroutine(CurrentGunBulletsLeft.Value));

        CurrentSecondGunTypeNet.Value = currentGunType;

    }

    private IEnumerator SwitchWeaponCoroutine(int newAmmo)
    {
        DespawnRpc();
        Debug.Log("WeaponCoroutine");

        SetUpGunRpc(CurrentSecondGunTypeNet.Value);

        yield return new WaitUntil(() => weapon != null);

        if (IsServer)
        {
            weapon.SetBullets(newAmmo);
            SyncWeaponAmmoClientRpc(newAmmo);
        }

    }

    [Rpc(SendTo.Owner)]
    private void SyncWeaponAmmoClientRpc(int ammo)
    {
        weapon.BulletsLeft = ammo;
        
    }


    public void GrabGun(GunType gunPicked){
       if(Gun == gunPicked || CurrentSecondGunTypeNet.Value == gunPicked) return;

        int actualScore = playerState.Score;
        GunScriptableObject gun = gunsList.Find(gun => gun.Type == gunPicked);

        if (actualScore < gun.scoreToBuy)
        {
            soundManager.PlaySound("CantBuyItem");
            return;
        }
        else
        {
            if (CurrentSecondGunTypeNet.Value != gunPicked)
            {
                if (CurrentSecondGunTypeNet.Value == CurrentGun.Type)
                {
                    ChangeSecondGunTypeRpc(CurrentGun.Type);
                }
                ChangeSecondGunTypeRpc(CurrentGun.Type);
                SendSecondaryGunBulletsLeftRpc(weapon.BulletsLeft);
                DespawnRpc();
                Gun = gunPicked;
                CurrentGun = gun.Clone() as GunScriptableObject;
                SendCurrentGunBulletsLeftRpc(CurrentGun.MagazineSize);
                SetUpGunRpc(gunPicked);
                MultiGameManager.Instance.PlayerScore(OwnerClientId, -(int)gun.scoreToBuy);
            }
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
        if (!IsOwner) return;

        if (context.started){
            if (!weapon.Realoading){
                RealoadGun();
            }
        }
    }
    private void RealoadGun(){
        if (CurrentGun.Type == GunType.ShinelessFeather) return;
        ReloadEventRpc();

        int totalAmmo = actualTotalAmmo.Value - (weapon.MagazineSize - weapon.BulletsLeft);
        SendActualAmmoRpc(totalAmmo);
        weapon.Reload();
        if (UIManager.Singleton)
        {
            UIManager.Singleton.GetPlayerTotalAmmo(totalAmmo);
        }
        //actualTotalAmmo.Value -= weapon.MagazineSize - weapon.BulletsLeft;
    }
    private IEnumerator Reload(float delay)
    {
        if (shooting) shooting = false;
        canShoot = false;
        yield return new WaitForSeconds(delay);
        StopFeedback();
        canShoot = true;
    }
    #endregion

    public void GainAmmo(int amount)
    {
        actualTotalAmmo.Value += amount;
    }
/// <summary>
/// Devuelve el arma que se le pida
/// </summary>
/// <param name="gunToFind">Tipo del arma que se quiere encontrar.</param>
/// <returns></returns>
    public GunScriptableObject GetGun(GunType gunToFind){
        return gunsList.Find(gun => gun.Type == gunToFind);
    }

    private void ShootingFeedback()
    {
        switch (CurrentGun.Type)
        {
            case GunType.Rifle:

                playerAnimator.SetBool("ShootBurst", true);
                break;
            case GunType.BasicPistol:

                playerAnimator.SetTrigger("ShootOnce");
                break;
            case GunType.Revolver:

                playerAnimator.SetTrigger("ShootOnce");
                break;
            case GunType.Shotgun:

                playerAnimator.SetTrigger("ShootOnce");
                break;
            case GunType.Sniper:

                playerAnimator.SetTrigger("ShootOnce");
                break;
        }
    }
    private void StopFeedback()
    {
        soundManager.StopSound("rifleFire");
        playerAnimator.SetBool("ShootBurst", false);

        soundManager.StopSound("rifleReload");
        soundManager.StopSound("pistolReload");
        soundManager.StopSound("revolverReload");
        soundManager.StopSound("shotgunReload");
        soundManager.StopSound("sniperReload");
    }
    private void ReloadingFeedback()
    {

        playerAnimator.SetTrigger("Reload");

        StartCoroutine(AnimLayerCountdown("Reload", 4.5f));

        switch (CurrentGun.Type)
        {
            case GunType.Rifle:
                soundManager.PlaySound("rifleReload");
                //StartCoroutine(Reload(2));
                break;
            case GunType.BasicPistol:
                soundManager.PlaySound("pistolReload");
                //StartCoroutine(Reload(2.12f));
                break;
            case GunType.Revolver:
                soundManager.PlaySound("revolverReload");
                //StartCoroutine(Reload(4.3f));
                break;
            case GunType.Shotgun:
                soundManager.PlaySound("shotgunReload");
                //StartCoroutine(Reload(5.4f));
                break;
            case GunType.Sniper:
                soundManager.PlaySound("sniperReload");
                //StartCoroutine(Reload(1.45f));
                break;
        }
        StartCoroutine(Reload(weapon.ReloadTime));

    }

    private IEnumerator AnimLayerCountdown(string layer, float delay)
    {
        float timer = 0f;
        int layerI = playerAnimator.GetLayerIndex(layer);
        playerAnimator.SetLayerWeight(layerI, 1);

        while (timer < delay)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        playerAnimator.SetLayerWeight(layerI, 0);
    }

    public void CheckZoomIn()
    {
        if (CurrentGun.Type == GunType.Sniper) crosshairManager.AimingZoomIn();
    }
    public void CheckZoomOut()
    {
        crosshairManager.AimingZoomOut();
    }
    public void ScaleDamage(int damageToAdd)
    {
        basePlayerDamage += damageToAdd;
    }
    [Rpc(SendTo.Server)]
    public void ReloadEventRpc()
    {
        ReloadEvent?.Invoke(OwnerClientId);
        Debug.Log("ReloadEvent");
    }
}
