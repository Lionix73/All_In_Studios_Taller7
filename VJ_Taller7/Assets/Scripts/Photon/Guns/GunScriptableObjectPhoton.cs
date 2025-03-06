using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "GunPhoton", menuName = "GunsPhoton/GunPhoton", order = 0)]
public class GunScriptableObjectPhoton : ScriptableObject {
    public GunType Type;
    public string Name;
    public Sprite UIImage;
    public NetworkObject ModelPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;
    public int Damage;
    public int MagazineSize;
    public float ReloadTime;
    public NetworkObject Model;
    public float LastShootTime;

    public ShootConfigScriptableObjtect ShootConfig;
    public TrailConfigScriptableObject TrailConfig;

    
    private MonoBehaviour ActiveMonoBehaviour;
    private Camera activeCamera;
    private int bulletsLeft;
    public int BulletsLeft {
        get => bulletsLeft;
        set {
            bulletsLeft = value;
        }
    }
    public bool realoading;
    public bool Realoading {
        get => realoading;
    }
    private ParticleSystem ShootSystem;
    private ObjectPool<TrailRenderer> TrailPool;
    private ObjectPool<Bullet> BulletPool;

    private void Awake() {
        bulletsLeft = MagazineSize;
    }

    public void Spawn(Transform Parent) {
        LastShootTime = 0f;
        if (bulletsLeft == 0){ //En revision porque no se recarga al recoger el arma, ni la primera vez que aparece.
        bulletsLeft = MagazineSize;}
        realoading = false;

        Model = Instantiate(ModelPrefab);
        Model.transform.SetParent(Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localEulerAngles = SpawnRotation;
    }

    public void DeSpawn(){
        //Destroy(Model);
        //Model.SetActive(false);
        Destroy(Model);
        TrailPool.Clear();
        if (BulletPool != null){
            BulletPool.Clear();
        }
    }

    public void Shoot(){
        if (Time.time > ShootConfig.FireRate + LastShootTime && bulletsLeft > 0 && !realoading){
            LastShootTime = Time.time;
            ShootSystem.Play();
            bulletsLeft -= ShootConfig.BulletsPerShot;

            for (int i = 0; i < ShootConfig.BulletsPerShot; i++){
                Vector3 shootDirection;
                if (ShootConfig.HaveSpread){
                    shootDirection = ShootSystem.transform.forward +
                    new Vector3(Random.Range
                    (-ShootConfig.Spread.x, ShootConfig.Spread.x),
                    Random.Range
                    (-ShootConfig.Spread.y, ShootConfig.Spread.y),
                    Random.Range
                    (-ShootConfig.Spread.z, ShootConfig.Spread.z)
                    );
                }
                else {
                    shootDirection = ShootSystem.transform.forward;
                }
                shootDirection.Normalize();

            }
        }
    }

    public Vector3 GetGunForward(){
        return Model.transform.forward;
    }
        
    public void Reload() {
        realoading = true;
        //Invoke("FinishedReload", ReloadTime);
        ActiveMonoBehaviour.StartCoroutine(ReloadingCoroutine());
    }
    private IEnumerator ReloadingCoroutine(){
        yield return new WaitForSeconds(ReloadTime);
        FinishedReload();
    }
    private void FinishedReload() {
        bulletsLeft = MagazineSize;
        realoading = false;
        //Debug.Log("Fin de la recarga");
    }

    ///<summary>
    ///Funcion de copia manual, asignar valores del objeto general a una instancia nueva
    ///evita errores de referencias compartidas.
    ///RECORDAR: Si se añaden nuevos valores al objeto, añadirlos a esta función.
    ///          Si se añaden nuevos objetos, añadirlos a esta función.
    ///NOTA: Me encantaría explicar a detalle la magia negra que hace esta función, 
    ///      pero no tengo idea real de como funciona. aunque si siguen los saltos que da
    ///      todo tiene sentido...
    ///
    ///</summary>

    public object Clone() {
        GunScriptableObjectPhoton clone = CreateInstance<GunScriptableObjectPhoton>();

        clone.MagazineSize = MagazineSize;
        clone.Damage = Damage;
        clone.ReloadTime = ReloadTime;
        clone.Type = Type;
        clone.Name = Name;
        clone.UIImage = UIImage;
        clone.ModelPrefab = ModelPrefab;
        clone.SpawnPoint = SpawnPoint;
        clone.SpawnRotation = SpawnRotation;
        return clone;
    }
}