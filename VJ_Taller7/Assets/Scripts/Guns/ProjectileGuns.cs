using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
public class ProjectileGuns : MonoBehaviour
{
    [Header("Bullet projectile")]
    public GameObject bullet;

    [Header("Shooting forces")]
    [SerializeField] private float shootForce;
    [SerializeField] private float upwardForce;

    [Header("Gun stats")]
    [SerializeField] private float timeBetweenShooting;
     [SerializeField] private float spread;
    [SerializeField] private float realoadTime;
    [SerializeField] private float timeBetweenShots;
    [SerializeField] private int magazineSize, bulletsPerTap, bulletsLeft, bulletsShot;
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
    }
    private void Update() {
        MyInput();

        if (ammunitionDisplay!=null){
            ammunitionDisplay.SetText(bulletsLeft/bulletsPerTap + "/" + magazineSize/bulletsPerTap );
        }
    }

    public void OnShoot(InputAction.CallbackContext context) {
        if (allowButtonHold){
            shooting = context.performed;
            }
        else {shooting = context.started;}
        //Debug.Log("Fase: " + shooting);
    }

    public void OnReload(InputAction.CallbackContext context){
        if (bulletsLeft<magazineSize && !realoading){
            Reload();
        }
        Debug.Log("Recargando");
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

        GameObject currentBullet = Instantiate (bullet, bulletSpawnPoint.position, Quaternion.identity);

        if (haveSpread){
            currentBullet.transform.forward = directionWithSpread.normalized;
            currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce,ForceMode.Impulse);
        }
        else {currentBullet.transform.forward = directionWithoutSpread.normalized;
        currentBullet .GetComponent<Rigidbody>().AddForce(directionWithoutSpread.normalized * shootForce,ForceMode.Impulse);}

        bulletsLeft--;
        bulletsShot++;

        if (allowInvoke){
            Invoke ("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }

        if (bulletsShot < bulletsPerTap && bulletsLeft > 0){
            Invoke("Shoot", timeBetweenShots);
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
        bulletsLeft = magazineSize;
        realoading = false;
        Debug.Log("Fin de la recarga");
    }

}
