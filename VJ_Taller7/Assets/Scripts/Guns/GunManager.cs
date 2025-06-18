using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Cinemachine;

[RequireComponent(typeof(CrosshairManager))]
public class GunManager : MonoBehaviour
{
    [Header("Camera")]
    public Camera Camera; public CinemachineBrain cinemachineBrain; GameObject playerObject;
    private PlayerController player;
    [Space][Header("Managers")]
    public CrosshairManager crosshairManager;
    [Space][Header("Ammo Info")]
    public int actualTotalAmmo; //Cuanta municion tiene el jugador
    [SerializeField] private int MaxTotalAmmo; //Cuanta municion puede llevar el jugador

    public TextMeshProUGUI totalAmmoDisplay; //UI de la municion total que le queda al jugador
    public TextMeshProUGUI ammunitionDisplay; //UI de la municion que le queda en el cargador

    [Space][Header("Gun General Info")]
    [SerializeField] private int basePlayerDamage; //Con este se escala el game
    [Tooltip("Lista de las armas que existen en el juego")]
    [SerializeField] private List<GunScriptableObject> gunsList;
    [SerializeField] private Transform gunParent;
    [Tooltip("Tipo de arma que posee el jugador, define con cuál empieza")]
    [SerializeField] public GunType Gun; //Tipo de arma que tiene el jugador
    private Transform secondHandGrabPoint; // la posicion a asignar
    private Transform secondHandRigTarget; //el Rig en sí

    [SerializeField] private bool inAPickeableGun;
    private GunType gunToPick;

    [SerializeField] private PickeableType pickeableToBuy; private GameObject colleactableToGrab; //Por no hacer desde el principio una heredada de pickeables... SANTI....
    [SerializeField] private bool inAPickeableCollectable; //true para coger municion, false para vida

    private bool shooting;
    private bool canShoot = true;
    public bool GunCanShoot
    {
        get => canShoot;
        set => canShoot = value;
    }

    [Space]
    [Header("Active Guns Info")]

    public GunScriptableObject CurrentGun;
    [SerializeField] private GunType CurrentSecondGunType;
    private int CurrentSecondaryGunBulletsLeft;

    private Vector3 dondePegaElRayDelArma;

    #region Events - Si, cree un monton de eventos... para mas placer
    public delegate void ReloadingEvent();
    public event ReloadingEvent ReloadEvent;

    public delegate void ChangeWeaponEvent();
    public event ChangeWeaponEvent ChangeGun;

    public delegate void StopFeebackEvent();
    public event StopFeebackEvent StopShootingFeeback;

    public delegate void ShootEvent();
    public event ShootEvent ShootingEvent;

    public delegate void BuyWeaponEvent(bool canBuy);
    public event BuyWeaponEvent buyWeaponEvent;

    public delegate void BuyItemEvent(PickeableType type);
    public event BuyItemEvent buyItemEvent;
    #endregion

    private void Awake() {
        cinemachineBrain = GameObject.Find("CinemachineBrain").GetComponent<CinemachineBrain>();
        Camera = cinemachineBrain.GetComponent<Camera>();
        playerObject = transform.parent.gameObject;
        GetPlayer(playerObject);

        actualTotalAmmo=MaxTotalAmmo/2;
        if (gunParent == null) {gunParent = this.transform;} //En caso de no tener asignado el punto de la mano donde aparece el arma, que la sostenga encima

        GunScriptableObject gun = gunsList.Find(gun => gun.Type == Gun);
        if (gun == null) {
            Debug.LogError($"No se ha encontrado el arma: {CurrentGun}");
            return;
        }
        SetUpGun(gun);
        CurrentSecondaryGunBulletsLeft = CurrentGun.MagazineSize;
        CurrentSecondGunType=Gun;
        inAPickeableGun=false;

        secondHandRigTarget = GameObject.Find("SecondHandGripRig_target").GetComponent<Transform>();

        crosshairManager = GetComponent<CrosshairManager>(); 
    }

    private void Update() {
        if (totalAmmoDisplay != null) {
            totalAmmoDisplay.SetText(actualTotalAmmo + "/" + MaxTotalAmmo);
            
        }
        if (UIManager.Singleton !=null)
        {
            UIManager.Singleton.GetPlayerTotalAmmo(actualTotalAmmo);
        }

        if (shooting && CurrentGun.BulletsLeft > 0) 
        {
            CurrentGun.Shoot();
        }
        // Automatic Reload
        else if (CurrentGun.BulletsLeft<=0 && !CurrentGun.Realoading && actualTotalAmmo > 0)
        {
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
        if (actualTotalAmmo < 0) actualTotalAmmo = 0;


        if (secondHandRigTarget == null) return;
        secondHandRigTarget.position = secondHandGrabPoint.position;
    }

    #region -----Set Up-----
    private void SetUpGun(GunScriptableObject gun){
        CurrentGun = gun.Clone() as GunScriptableObject;
        CurrentGun.Spawn(gunParent, this, Camera);

        CurrentGun.Damage += basePlayerDamage;
        if (UIManager.Singleton != null)
        {
            UIManager.Singleton.GetPlayerGunInfo(CurrentGun.BulletsLeft, CurrentGun.MagazineSize, CurrentGun);
        }

        SetUpGunRigs();

        if (crosshairManager == null) return;
        if (CurrentGun.CrosshairImage == null) return;
        crosshairManager.SetCrosshairImage(CurrentGun.CrosshairImage);

        if (player!= null)
        player.gameObject.GetComponent<FOVHandler>().SetAimFOV(CurrentGun.aimFov);

        Debug.Log($"Zoom set to: {CurrentGun.aimFov}");
    }
    private void SetUpGunRigs(){
        Transform[] chGun = gunParent.GetComponentsInChildren<Transform>();
        //secondHandGrabPoint = GameObject.Find("SecondHanGrip").transform;
        for(int i = 0; i < chGun.Length; i++)
        {
            if (chGun[i].name == "SecondHandGrip") {
                secondHandGrabPoint = chGun[i].transform;
            }
        }
    }
    #endregion

    public void OnShoot(InputAction.CallbackContext context) //RECORDAR ASIGNAR MANUALMENTE EN LOS EVENTOS DEL INPUT
    {
        UIManager ui = UIManager.Singleton;
        if (ui!=null){ 
            if (ui.IsPaused || ui.IsMainMenu || ui.IsDead)
            {
                canShoot = false;
            }
            else
                canShoot = true;
        }
        if (!canShoot || CurrentGun.realoading) return;

        if(context.started && CurrentGun.bulletsLeft < 1)
        {
            ShootingEvent?.Invoke();
            return;
        }

        if (context.started && CurrentGun.ShootConfig.IsAutomatic)
        {
            shooting = true;
            ShootingEvent?.Invoke();
        }
        else if (!CurrentGun.ShootConfig.IsAutomatic)
        {
            shooting = context.started;

            if(shooting) ShootingEvent?.Invoke();
        }

        if (context.canceled && CurrentGun.ShootConfig.IsAutomatic)
        {
            shooting = false;
            StopShootingFeeback?.Invoke();
        }
    }

    #region -----Weapon Change-----
    public void OnWeaponChange(InputAction.CallbackContext context){
        if (context.started){
            if (CurrentSecondGunType!=CurrentGun.Type)
            {
                ChangeGun?.Invoke();
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

    public void DespawnActiveGun(){
        if (CurrentGun!=null){
            CurrentGun.DeSpawn();
        }
        Destroy(CurrentGun);
    }
    #endregion

    #region -----Grab Gun-----
    public void GrabGun(GunType gunPicked){
        float actualScore = GameManager.Instance.scoreManager.GetScore();
        GunScriptableObject gun = gunsList.Find(gun => gun.Type == gunPicked);

        if (actualScore < gun.scoreToBuy)
        {
            CannottBuyItem();
            return;
        }

        if (CurrentSecondGunType != gunPicked)
        {
            if (CurrentSecondGunType == CurrentGun.Type)
            {
                CurrentSecondGunType = CurrentGun.Type;
            }

            DespawnActiveGun();
            this.Gun = gunPicked;
            CanBuyWeapon();

            SetUpGun(gun);

            GameManager.Instance.scoreManager.SetScore(-gun.scoreToBuy);
        }
    }
    public void OnGrabGun(InputAction.CallbackContext context){
        if (context.started)
        {
            if (inAPickeableGun)
            {
                GrabGun(gunToPick);
            }
            else if (inAPickeableCollectable)
            {
                GrabCollectable();
            }
            else
            {
                GameManager.Instance.roundManager.OrderToPassRound(); //Asegurarnos que no pasen de ronda sin querer
            }
        }
    }
    #endregion

    #region -----Pickeables Management-----
    //Aqui sabemos si el jugador esta cerca de un arma, que tipo de arma es y si puede recogerla
    public void EnterPickeableGun(GunType gunType){
        gunToPick = gunType;
        inAPickeableGun = true;
    }

    public void ExitPickeableGun(){
        inAPickeableGun = false;
    }

    public void CanBuyWeapon() => buyWeaponEvent?.Invoke(true);

    public void CanBuyItem(PickeableType type) => buyItemEvent?.Invoke(type);

    public void CannottBuyItem() => buyWeaponEvent?.Invoke(false);

    public void EnterPickeableCollectable(PickeableType typeToPick, GameObject colleactable)
    {
        inAPickeableCollectable = true;

        colleactableToGrab = colleactable;
        pickeableToBuy = typeToPick;
    }
    public void ExitPickeableCollectable()
    {
        inAPickeableCollectable = false;
    }

    public void GrabCollectable()
    {
        switch (pickeableToBuy)
        {
            case PickeableType.Ammo:
                colleactableToGrab.GetComponent<AmmoPickable>()?.BuyCollectable();
                return;
            case PickeableType.Healing:
                colleactableToGrab.GetComponent<HealthPickable>()?.BuyCollectable();
                return;
        }
    }
    #endregion

    #region -----Ammo Management-----
    //Aqui recibimos el input de recarga y llamamos a la funciond de recarga
    public void OnReload(InputAction.CallbackContext context)
    {
        if (CurrentGun.Type == GunType.ShinelessFeather) return;

        if (context.started)
        {
            if (!CurrentGun.Realoading && actualTotalAmmo > 0 && CurrentGun.bulletsLeft < CurrentGun.MagazineSize)
            {
                RealoadGun();
            }
        }
    }
    private void RealoadGun()
    {
        ReloadEvent?.Invoke();

        CurrentGun.Reload(actualTotalAmmo);
        actualTotalAmmo -= CurrentGun.MagazineSize - CurrentGun.BulletsLeft;

        StartCoroutine(ReloadDelay(CurrentGun.ReloadTime));
    }

    private IEnumerator ReloadDelay(float delay)
    {
        if(shooting) shooting = false;
        canShoot = false;
        yield return new WaitForSeconds(delay);
        canShoot = true;
    }
    #endregion

    #region -----Get Objects-----
    /// <summary>
    /// Devuelve el arma que se le pida
    /// </summary>
    /// <param name="gunToFind">Tipo del arma que se quiere encontrar.</param>
    /// <returns></returns>
    public GunScriptableObject GetGun(GunType gunToFind){
        return gunsList.Find(gun => gun.Type == gunToFind);
    }

    private void GetPlayer(GameObject activePlayer){
        player = activePlayer.GetComponentInChildren<PlayerController>();
    }
    #endregion

    #region -----Zoom-----
    public void CheckZoomIn(){
        if (CurrentGun.Type == GunType.Sniper) crosshairManager.AimingZoomIn(); 
    }
    public void CheckZoomOut(){
        crosshairManager.AimingZoomOut();
    }
    #endregion

    public void ScaleDamage(int damageToAdd)
    {
        basePlayerDamage += damageToAdd;
    }

    private void OnDrawGizmos() {
        if (CurrentGun == null) return;
        Gizmos.color = Color.red;
        dondePegaElRayDelArma = CurrentGun.dondePegaElRayoPaDisparar;
        Gizmos.DrawRay(CurrentGun.GetRaycastOrigin(), CurrentGun.GetGunForward() * CurrentGun.TrailConfig.MissDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(dondePegaElRayDelArma, 0.3f);
    }
}
