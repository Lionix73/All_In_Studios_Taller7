using System.Collections;
using System.Security.Cryptography;
using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunScriptableObject : ScriptableObject {
    public GunType Type;
    public string Name;

    public float scoreToBuy;
    public Sprite UIImage;
    public Sprite CrosshairImage;
    public GameObject ModelPrefab;
    public GameObject NetworkModelPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;
    public int Damage;
    public int MagazineSize;
    public float ReloadTime;
    [Tooltip("Especificar el zoom que tiene cada arma")]
    public float aimFov;

    public GameObject ImpactBulletEffectForHitScan;
    public ShootConfigScriptableObjtect ShootConfig;
    public TrailConfigScriptableObject TrailConfig;

    public MonoBehaviour ActiveMonoBehaviour;
    public GameObject Model;
    public Camera activeCamera;
    public float LastShootTime;
    public int bulletsLeft;
    public bool noFriendsInWar;
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
    public ObjectPool<Bullet> GFeatherPool;
    public ObjectPool<Bullet> SFeatherPool;

    public Vector3 dondePegaElRayoPaDisparar;

    [Header("Animator")]
    public RuntimeAnimatorController AnimatorController;

    [Header("Audio")]
    public EventReference ShootSound;
    public EventReference ReloadSound;
    public EventReference NoAmmoSound;
    
    [Header("Vibration")]
    [Range(0, 1)] public float ShootingVibrationIntensity;
    public float VibrationDuration;

    private HitFeedback hitFeedback;

    private void Awake()
    {
        bulletsLeft = MagazineSize;
    }

    public void Spawn(Transform Parent, MonoBehaviour ActiveMonoBehaviour, Camera camera =null) {
        this.ActiveMonoBehaviour = ActiveMonoBehaviour;
        LastShootTime = 0f;
        if (bulletsLeft == 0){ //En revision porque no se recarga al recoger el arma, ni la primera vez que aparece.
        bulletsLeft = MagazineSize;}
        realoading = false;
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);
        if (!ShootConfig.IsHitScan)
        {BulletPool = new ObjectPool<Bullet>(CreateBullet);}
        if(ShootConfig.ShootingType == ShootType.Special){
            GFeatherPool = new ObjectPool<Bullet>(CreateBullet);
            SFeatherPool = new ObjectPool<Bullet>(CreateBullet);
        }

        Model = Instantiate(ModelPrefab);
        Model.transform.SetParent(Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localEulerAngles = SpawnRotation;

        activeCamera = camera;

        ShootSystem = Model.GetComponentInChildren<ParticleSystem>();
        hitFeedback = FindFirstObjectByType<HitFeedback>();
    }

    public void DeSpawn(){
        //Destroy(Model);
        Model.SetActive(false);
        Destroy(Model);
        TrailPool.Clear();
        if (!ShootConfig.IsHitScan){
            BulletPool.Clear();
        }
        if(ShootConfig.ShootingType == ShootType.Special){
            GFeatherPool?.Clear();
            SFeatherPool?.Clear();
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

                if (ShootConfig.ShootingType == ShootType.HitScan)
                DoHitScanShooting(shootDirection, ShootSystem.transform.position, ShootSystem.transform.position);
                else
                {
                    switch (ShootConfig.ShootingType)
                    {
                        case ShootType.HitScan:

                            return;
                        case ShootType.Projectile:
                            DoProjectileShooting(shootDirection);
                            return;
                        case ShootType.Special:
                            DoSpecialShooting(shootDirection);
                            return;
                    }
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
            if (hit.collider.TryGetComponent(out IDamageable enemy))
            {
                enemy.TakeDamage(Damage);

                hitFeedback.ShowHitMarker();
            }
            else if (hit.collider.TryGetComponent(out EnemyHealthMulti enemyM))
            {
                enemyM.TakeDamageRpc(Damage);

            }
        }
        else
        {
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

    private void DoProjectileShooting(Vector3 ShootDirection){
        Bullet bullet = BulletPool.Get();

        bullet.gameObject.SetActive(true);
        bullet.OnCollision += HandleBulletCollision;
        bullet.transform.position = ShootSystem.transform.position;
        bullet.Spawn(ShootDirection* ShootConfig.BulletSpawnForce);

        // El trail de la bala podemos decidir si ponerlo en la bala, o usar el mismo pool que con el hitscan

    }

    private void DoSpecialShooting(Vector3 ShootDirection){
        switch(Type){
            case GunType.MysticCanon:
            return;
            case GunType.GoldenFeather:
            Bullet feather = GFeatherPool.Get();
            feather.gameObject.SetActive(true);
            feather.OnCollision += HandleBulletCollision;
            feather.OnBulletEnd += HandleGoldenBulletCollision; //Para poder eliminar tambien las balas especiales 
            feather.transform.position = ShootSystem.transform.position;
            feather.Spawn(ShootDirection* ShootConfig.BulletSpawnForce);
            return;
            case GunType.ShinelessFeather:
            Bullet shineless = SFeatherPool.Get();

            shineless.gameObject.SetActive(true);
            shineless.OnCollision += HandleBulletCollision;
            shineless.OnBulletEnd += HandleShinelessFeather; //Para poder eliminar tambien las balas especiales 
            shineless.transform.position = ShootSystem.transform.position;
            shineless.Spawn(ShootDirection* ShootConfig.BulletSpawnForce);
            return;
        }
    }
        
    public void Reload(int totalBulletsLeft) {
        realoading = true;
        //Invoke("FinishedReload", ReloadTime);
        ActiveMonoBehaviour.StartCoroutine(ReloadingCoroutine(totalBulletsLeft));
    }
    private IEnumerator ReloadingCoroutine(int totalBulletsLeft){
        yield return new WaitForSeconds(ReloadTime);
        FinishedReload(totalBulletsLeft);
    }
    private void FinishedReload(int totalBulletsLeft) {
        if (totalBulletsLeft > MagazineSize) bulletsLeft = MagazineSize;
        else if (bulletsLeft + totalBulletsLeft >= MagazineSize) bulletsLeft = MagazineSize;
        else bulletsLeft = totalBulletsLeft; 
        // Si no tengo suficiente para el cargador, pos me quedo con las pocas que tenga
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

        if (ImpactBulletEffectForHitScan != null)
            {
                GameObject impact = Instantiate(ImpactBulletEffectForHitScan, EndPoint, Quaternion.identity);
                impact.transform.up = Hit.normal;
                Destroy(impact, 0.5f);
            }

        instance.transform.position = EndPoint;

        yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        TrailPool.Release(instance);
    }


    private void HandleBulletCollision(Bullet bullet, Collision collision){
        // En caso de usar la pool de trail, hay que desactivarla desde aqui

        if (ShootConfig.ShootingType != ShootType.Special){
            bullet.gameObject.SetActive(false);
            BulletPool.Release(bullet);
        }

        if (collision != null){
            ContactPoint contactPoint = collision.GetContact(0);

            Collider colliderHit = contactPoint.otherCollider;

            if(noFriendsInWar && colliderHit.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                if (colliderHit.TryGetComponent(out IDamageable player))
                {
                    player.TakeDamage(Damage);
                }
            }

            if (colliderHit.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;

            if (colliderHit.TryGetComponent(out IDamageable enemy))
            {
                enemy.TakeDamage(Damage);
                hitFeedback.ShowHitMarker();
            }
            else if(colliderHit.TryGetComponent(out EnemyHealthMulti enemyM))
            {
                enemyM.TakeDamageRpc(Damage);
            }
        }
    }
    private void HandleGoldenBulletCollision(Bullet bullet, Collision collision)
    {
        bullet.gameObject.SetActive(false);
        GFeatherPool.Release(bullet);
        bullet.OnCollision -= HandleBulletCollision;
        bullet.OnBulletEnd -= HandleGoldenBulletCollision;
    }
    private void HandleShinelessFeather(Bullet bullet, Collision collision)
    {
        bullet.gameObject.SetActive(false);
        GFeatherPool.Release(bullet);
        bulletsLeft = 1;
        
        bullet.OnCollision -= HandleBulletCollision;
        bullet.OnBulletEnd -= HandleShinelessFeather;
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

    private Bullet CreateBullet()
    {
        return Instantiate(ShootConfig.BulletPrefab);
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
        clone.NetworkModelPrefab = NetworkModelPrefab;
        clone.SpawnPoint = SpawnPoint;
        clone.SpawnRotation = SpawnRotation;
        clone.aimFov = aimFov;
        clone.ImpactBulletEffectForHitScan = ImpactBulletEffectForHitScan;
        clone.ShootSound = ShootSound;
        clone.ReloadSound = ReloadSound;
        clone.NoAmmoSound = NoAmmoSound;
        clone.ShootingVibrationIntensity = ShootingVibrationIntensity;
        clone.VibrationDuration = VibrationDuration;
        return clone;
    }
}