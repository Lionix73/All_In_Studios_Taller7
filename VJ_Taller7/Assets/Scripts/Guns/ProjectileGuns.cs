using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;
public class ProjectileGuns : MonoBehaviour
{
    [Header("Bullet projectile")]
    public List<GameObject> bullets = new List<GameObject>();
    public GameObject bullet;
    private int currentBulletIndex;
    //private GameObject currentBullet;

    [Header("Shooting forces")]
    [SerializeField] private float shootForce;
    [SerializeField] private float upwardForce;

    [Header("Gun stats")]
    [SerializeField] private float timeBetweenShooting;
     [SerializeField] private float spread;
    [SerializeField] private float realoadTime;
    [SerializeField] private float timeBetweenShots;
    [SerializeField] private int magazineSize, bulletsPerTap;
    private int bulletsLeft, bulletsShot;
    [SerializeField] private bool allowButtonHold, haveSpread;
    [SerializeField] private int maxDistanceTarget; //en caso de no apuntar a nada, fija un punto a esta distancia

    private bool shooting, readyToShoot, realoading;
    private bool allowInvoke = true;

    public Camera playerCamera;
    public Transform bulletSpawnPoint;

    [Header("Graphics")]
    public GameObject muzzleFlash;
    public TextMeshProUGUI ammunitionDisplay;

    private void Awake() {
        bulletsLeft = magazineSize;
        readyToShoot = true;
        foreach(var bullet in bullets) {
            bullet.SetActive(false);
        }
        currentBulletIndex =0;
    }
    private void Update() {
        MyInput();

        if (ammunitionDisplay!=null){
            ammunitionDisplay.SetText(bulletsLeft/bulletsPerTap + "/" + magazineSize/bulletsPerTap );
        }
    }

    public void OnShoot(InputAction.CallbackContext context) { //RECORDAR ASIGNAR MANUALMENTE EN LOS EVENTOS DEL INPUT
        if (allowButtonHold){
            shooting = context.performed;
            }
        else {shooting = context.started;}
        //Debug.Log("Fase: " + shooting);
    }

    public void OnReload(InputAction.CallbackContext context){ //RECORDAR ASIGNAR MANUALMENTE EN LOS EVENTOS DEL INPUT
        if (bulletsLeft<magazineSize && !realoading){
            Reload();
        }
        //Debug.Log("Recargando");
    }

    public void MyInput(){
        /* //viejo intput system
        if (allowButtonHold) {
            shooting = Input.GetKey(KeyCode.Mouse0);
            }
            else {shooting = Input.GetKeyDown(KeyCode.Mouse0);}
        */

        
        //recarga automatica
        if (readyToShoot && shooting && !realoading && bulletsLeft<=0){Reload();}

        if (readyToShoot && shooting && !realoading && bulletsLeft >0){
            bulletsShot = 0;
            Shoot();
        }
    }

    private void Shoot(){
        readyToShoot = false;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit)){
            targetPoint = hit.point;
        }
        else { targetPoint = ray.GetPoint(maxDistanceTarget);}


        Vector3 directionWithoutSpread = targetPoint - bulletSpawnPoint.position;

        float x = Random.Range(-spread,spread);
        float y = Random.Range(-spread,spread);
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0);


        
        GameObject currentBullet = Instantiate(bullet, bulletSpawnPoint.position, Quaternion.identity);
        //bullets[currentBulletIndex].SetActive(true);
        //currentBullet = bullets[currentBulletIndex];
        currentBullet.transform.position=bulletSpawnPoint.position;
        //currentBullet.transform.rotation=Quaternion.identity;


        if (haveSpread){
            currentBullet.transform.forward = directionWithSpread.normalized;
            currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce,ForceMode.Impulse);
        }
        else {currentBullet.transform.forward = directionWithoutSpread.normalized;
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithoutSpread.normalized * shootForce,ForceMode.Impulse);}

        bulletsLeft--;
        bulletsShot++;
        currentBulletIndex++;

        if (allowInvoke){
            Invoke ("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }

        if (bulletsShot < bulletsPerTap && bulletsLeft > 0){
            Invoke("Shoot", timeBetweenShots);
            //Debug.Log("piu, pew, piw");
        }
    }

    private void ResetShot(){
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload(){
        realoading = true;
        Invoke("FinishedReload", realoadTime);
    }
    private void FinishedReload(){
        bulletsLeft = magazineSize; currentBulletIndex=0;
        realoading = false;
        //Debug.Log("Fin de la recarga");
    }

}
