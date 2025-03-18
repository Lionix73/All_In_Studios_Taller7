using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunScriptableObject : ScriptableObject {
    public GunType Type;
    public string Name;
    public Sprite UIImage;
    public Sprite CrosshairImage;
    public GameObject ModelPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;
    public int Damage;
    public int MagazineSize;
    public float ReloadTime;

    public ShootConfigScriptableObjtect ShootConfig;
    public TrailConfigScriptableObject TrailConfig;

    
    public MonoBehaviour ActiveMonoBehaviour;
    public GameObject Model;
    public Camera activeCamera;
    public float LastShootTime;
    public int bulletsLeft;
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
    public ParticleSystem ShootSystem;
    public ObjectPool<TrailRenderer> TrailPool;
    public ObjectPool<Bullet> BulletPool;

    public Vector3 dondePegaElRayoPaDisparar;

    private void Awake() {
        bulletsLeft = MagazineSize;
    }

    public void Spawn(Transform Parent, MonoBehaviour ActiveMonoBehaviour, Camera camera =null) {
        this.ActiveMonoBehaviour = ActiveMonoBehaviour;
        LastShootTime = 0f;
        if (bulletsLeft == 0){ //En revision porque no se recarga al recoger el arma, ni la primera vez que aparece.
        bulletsLeft = MagazineSize;}
        realoading = false;
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);

        Model = Instantiate(ModelPrefab);
        Model.transform.SetParent(Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localEulerAngles = SpawnRotation;

        activeCamera = camera;

        ShootSystem = Model.GetComponentInChildren<ParticleSystem>();
    }

    public void DeSpawn(){
        //Destroy(Model);
        Model.SetActive(false);
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

                if (ShootConfig.IsHitScan){
                    DoHitScanShooting(shootDirection, ShootSystem.transform.position, ShootSystem.transform.position);
                }
                else {
                    //DoProjectileShooting();
                }
            }
        }
    }

    private void DoHitScanShooting(Vector3 shootDirection, Vector3 Origin, Vector3 TrailOrigin, int Iteration = 0){
        if (Physics.Raycast(Origin, 
                            shootDirection, 
                            out RaycastHit hit, 
                            float.MaxValue, 
                            ShootConfig.HitMask))
                {
                ActiveMonoBehaviour.StartCoroutine(PlayTrail(TrailOrigin, hit.point, hit));
                dondePegaElRayoPaDisparar = hit.point;
                if (hit.collider.TryGetComponent(out Enemy enemey)){
                    enemey.TakeDamage(Damage);
                }
                else if(hit.collider.TryGetComponent(out EnemyHealthMulti enemy))
                {
                    enemy.TakeDamageRpc(Damage);
                
                }

            }
            else {  
                ActiveMonoBehaviour.StartCoroutine(PlayTrail(
                    TrailOrigin,
                    TrailOrigin + (shootDirection * TrailConfig.MissDistance),
                    new RaycastHit())
                    );
            }
    }

    public Vector3 GetRaycastOrigin(){
        Vector3 origin = ShootSystem.transform.position; //si dispara desde el arma

        origin = activeCamera.transform.position +
                 activeCamera.transform.forward * Vector3.Distance(
                    activeCamera.transform.position, ShootSystem.transform.position
                 );
        return origin;
    }
    public Vector3 GetGunForward(){
        return Model.transform.forward;
    }

    private void DoProjectileShooting(Vector3 shootDirection){
    
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

    private IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit){
        TrailRenderer instance = TrailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = StartPoint;
        yield return null; //Evitar sobreposicion de trails

        instance.emitting = true;

        float distance = Vector3.Distance(StartPoint, EndPoint);
        float remainingDistance = distance;
        while (remainingDistance > 0){
            instance.transform.position = Vector3.Lerp(
                StartPoint, 
                EndPoint, 
                Mathf.Clamp01(1- (remainingDistance / distance))
            );
            remainingDistance -= TrailConfig.SimulationSpeed * Time.deltaTime;
            yield return null;
        }

        instance.transform.position = EndPoint;

        yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        TrailPool.Release(instance);
    }




    public TrailRenderer CreateTrail() {
        GameObject instance = new GameObject("Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();

        trail.colorGradient = TrailConfig.ColorGradient;
        trail.material = TrailConfig.TrailMaterial;
        trail.widthCurve = TrailConfig.WidthCurve;
        trail.time = TrailConfig.Duration;
        trail.minVertexDistance = TrailConfig.MinVertexDistance;

        trail.emitting = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        return trail;
    }

    ///<summary>
    ///Funcion de copia manual, asignar valores del objeto general a una instancia nueva
    ///evita errores de referencias compartidas.
    ///RECORDAR: Si se añaden nuevos valores al objeto, añadirlos a esta función.
    ///          Si se añaden nuevos objetos, añadirlos a esta función.
    ///NOTA: Me encantaría explicar a detalle la magia negra que hace esta función, 
    ///      pero no tengo idea real de como funciona. aunque si siguen los saltos que da
    ///      todo tiene sentido...
    ///</summary>

    public object Clone() {
        GunScriptableObject clone = CreateInstance<GunScriptableObject>();

        clone.ShootConfig = ShootConfig.Clone() as ShootConfigScriptableObjtect;
        clone.TrailConfig = TrailConfig.Clone() as TrailConfigScriptableObject;

        clone.MagazineSize = MagazineSize;
        clone.Damage = Damage;
        clone.ReloadTime = ReloadTime;
        clone.Type = Type;
        clone.Name = Name;
        clone.UIImage = UIImage;
        clone.CrosshairImage = CrosshairImage;
        clone.ModelPrefab = ModelPrefab;
        clone.SpawnPoint = SpawnPoint;
        clone.SpawnRotation = SpawnRotation;
        return clone;
    }
}