using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Cinemachine;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using NUnit.Framework;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;

[RequireComponent(typeof(CrosshairManager))]
public class GunManager : MonoBehaviour
{
    [Header("Camera")]
    public Camera Camera; public CinemachineBrain cinemachineBrain; GameObject playerObject;
    private PlayerController player;
    [Space][Header("Managers")]
    public CrosshairManager crosshairManager;  
    private ThisObjectSounds soundManager; private Animator playerAnimator;
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
    public bool canShoot = true;

    [Space]
    [Header("Active Guns Info")]

    public GunScriptableObject CurrentGun;
    [SerializeField] private GunType CurrentSecondGunType;
    private int CurrentSecondaryGunBulletsLeft;

    private Vector3 dondePegaElRayDelArma;

    public delegate void ReloadingEvent();
    public event ReloadingEvent ReloadEvent;

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


        if (secondHandRigTarget==null) return;
        secondHandRigTarget.position = secondHandGrabPoint.position;
    }

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
        player.SetAimFOV(CurrentGun.aimFov);

        Debug.Log($"Zoom set to: {CurrentGun.aimFov}");
    }

    public void DespawnActiveGun(){
        if (CurrentGun!=null){
            CurrentGun.DeSpawn();
        }
        Destroy(CurrentGun);
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

    public void OnShoot(InputAction.CallbackContext context) { //RECORDAR ASIGNAR MANUALMENTE EN LOS EVENTOS DEL INPUT
        //if (CurrentGun.ShootConfig.IsAutomatic) {
        //    shooting = context.performed;
        //}
        //else { shooting = context.started; }

        UIManager ui = UIManager.Singleton;
        if (ui!=null){ 
            if (ui.IsPaused || ui.IsMainMenu || ui.IsDead)
            {
                canShoot = false;
            }
            else canShoot = true;
            //else if (!ui.IsPaused && !ui.IsMainMenu && !ui.IsDead)
            //{
            //    canShoot = true;
            //}
        }
        if (!canShoot) return;

        if (context.started && CurrentGun.ShootConfig.IsAutomatic) 
            shooting=true;
        else if (!CurrentGun.ShootConfig.IsAutomatic) shooting = context.started;

        if (context.canceled && CurrentGun.ShootConfig.IsAutomatic){
            shooting = false; StopFeedback();
            //Debug.Log("Fase: " + shooting);
        }

        if (context.started) ShootingFeedback();
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
        float actualScore = GameManager.Instance.scoreManager.GetScore();
        GunScriptableObject gun = gunsList.Find(gun => gun.Type == gunPicked);

        if (actualScore < gun.scoreToBuy)
        {
            soundManager.PlaySound("CantBuyItem");
            return;
        }

        if (CurrentSecondGunType != gunPicked){
            if (CurrentSecondGunType == CurrentGun.Type){
                CurrentSecondGunType = CurrentGun.Type;
            }
            DespawnActiveGun();
            this.Gun = gunPicked;
            soundManager.PlaySound("BuyItem");
            
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

    #region Ammo management //Aqui recibimos el input de recarga y llamamos a la funciond de recarga
    public void OnReload(InputAction.CallbackContext context)
    {

        if (context.started)
        {
            if (!CurrentGun.Realoading)
            {
                RealoadGun();
            }
        }
    }
    private void RealoadGun(){
        if (CurrentGun.Type == GunType.ShinelessFeather) return;
        ReloadEvent?.Invoke();

        StopFeedback();
        CurrentGun.Reload();
        actualTotalAmmo -= CurrentGun.MagazineSize - CurrentGun.BulletsLeft;
        ReloadingFeedback();
        //StartCoroutine(Reload(CurrentGun.ReloadTime)); //Deberia cordinarse la animacion al tiempo de recarga de las armas
        /*
        switch (Gun)
        {
            case GunType.Rifle:
                StartCoroutine(Reload(2));
                break;
            case GunType.BasicPistol:
                StartCoroutine(Reload(2.12f));
                break;
            case GunType.Revolver:
                StartCoroutine(Reload(4.3f));
                break;
            case GunType.Shotgun:
                StartCoroutine(Reload(5.4f));
                break;
            case GunType.Sniper:
                StartCoroutine(Reload(1.45f));
                break;
        }
        */
    }

    private IEnumerator Reload(float delay)
    {
        if(shooting) shooting = false;
        canShoot = false;
        yield return new WaitForSeconds(delay);
        StopFeedback();
        canShoot = true;
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

    private void GetPlayer(GameObject activePlayer){
        player = activePlayer.GetComponentInChildren<PlayerController>();
        playerAnimator = activePlayer.GetComponentInChildren<Animator>();
        soundManager = activePlayer.GetComponentInChildren<ThisObjectSounds>();
    }

    private void OnDrawGizmos() {
        if (CurrentGun == null) return;
        Gizmos.color = Color.red;
        dondePegaElRayDelArma = CurrentGun.dondePegaElRayoPaDisparar;
        Gizmos.DrawRay(CurrentGun.GetRaycastOrigin(), CurrentGun.GetGunForward() * CurrentGun.TrailConfig.MissDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(dondePegaElRayDelArma, 0.3f);
    }

    private void ShootingFeedback(){
        switch(CurrentGun.Type)
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
    private void StopFeedback(){
        soundManager.StopSound("rifleFire");
        playerAnimator.SetBool("ShootBurst", false);

        soundManager.StopSound("rifleReload", "pistolReload", "revolverReload", "shotgunReload", "sniperReload");
    }
    private void ReloadingFeedback(){
        
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
            StartCoroutine(Reload(CurrentGun.ReloadTime));
        
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

    public void CheckZoomIn(){
        if (CurrentGun.Type == GunType.Sniper) crosshairManager.AimingZoomIn(); 
    }
    public void CheckZoomOut(){
        crosshairManager.AimingZoomOut();
    }

    public void ScaleDamage(int damageToAdd){
        basePlayerDamage += damageToAdd;
    }
}
