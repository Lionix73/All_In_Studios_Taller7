using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Pool;

public class WeaponLogic : NetworkBehaviour
{
    GunManagerMulti2 gunManager;
    public GunScriptableObject currentGun;

    public GunType Type;
    public string Name;
    public Sprite UIImage;
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
    public int BulletsLeft
    {
        get => bulletsLeft;
        set
        {
            bulletsLeft = value;
        }
    }
    public bool realoading;
    public bool Realoading
    {
        get => realoading;
    }
    public ParticleSystem ShootSystem;
    public ObjectPool<TrailRenderer> TrailPool;
    public ObjectPool<Bullet> BulletPool;

    public void SetBullets(int amount)
    {
        if (IsServer)
        {
            bulletsLeft = amount;
        }
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    
        activeCamera = Camera.main;


        bulletsLeft = currentGun.bulletsLeft;
        MagazineSize = currentGun.MagazineSize;

        Type = currentGun.Type;
        name = currentGun.Name;
        UIImage = currentGun.UIImage;
        ModelPrefab = currentGun.ModelPrefab;
        SpawnPoint = currentGun.SpawnPoint;
        SpawnRotation = currentGun.SpawnRotation;
        Damage = currentGun.Damage;
        ReloadTime = currentGun.ReloadTime;

        ShootConfig = currentGun.ShootConfig;
        TrailConfig = currentGun.TrailConfig;

        //ActiveMonoBehaviour = currentGun.ActiveMonoBehaviour; //Este es el que entiendo que debe dar porblemas
        LastShootTime = currentGun.LastShootTime;
        
        realoading = currentGun.Realoading;
        //TrailPool = currentGun.TrailPool;
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);
        BulletPool = currentGun.BulletPool;
    }

    public void Spawn(Transform Parent, MonoBehaviour ActiveMonoBehaviour, Camera camera = null)
    {
        this.ActiveMonoBehaviour = ActiveMonoBehaviour;
        LastShootTime = 0f;
        if (bulletsLeft == 0)
        { //En revision porque no se recarga al recoger el arma, ni la primera vez que aparece.
            bulletsLeft = MagazineSize;
        }
        realoading = false;
        

        Model = Instantiate(ModelPrefab);
        Model.transform.SetParent(Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localEulerAngles = SpawnRotation;

        activeCamera = camera;

        ShootSystem = Model.GetComponentInChildren<ParticleSystem>();
    }

    public void DeSpawn()
    {
        //Destroy(Model);
        Model.SetActive(false);
        Destroy(Model);
        TrailPool.Clear();
        if (BulletPool != null)
        {
            BulletPool.Clear();
        }
    }

    public void Shoot()
    {
        if (Time.time > ShootConfig.FireRate + LastShootTime && bulletsLeft > 0 && !realoading)
        {
            LastShootTime = Time.time;
            ShootSystem.Play();
            bulletsLeft -= ShootConfig.BulletsPerShot;

            for (int i = 0; i < ShootConfig.BulletsPerShot; i++)
            {
                Vector3 shootDirection;
                if (ShootConfig.HaveSpread)
                {
                    shootDirection = ShootSystem.transform.forward +
                    new Vector3(Random.Range
                    (-ShootConfig.Spread.x, ShootConfig.Spread.x),
                    Random.Range
                    (-ShootConfig.Spread.y, ShootConfig.Spread.y),
                    Random.Range
                    (-ShootConfig.Spread.z, ShootConfig.Spread.z)
                    );
                }
                else
                {
                    shootDirection = ShootSystem.transform.forward;
                }
                shootDirection.Normalize();

                if (ShootConfig.IsHitScan)
                {
                    DoHitScanShooting(shootDirection, ShootSystem.transform.position, ShootSystem.transform.position);
                }
                else
                {
                    //DoProjectileShooting();
                }
            }
        }
    }

    private void DoHitScanShooting(Vector3 shootDirection, Vector3 Origin, Vector3 TrailOrigin, int Iteration = 0)
    {
        if (Physics.Raycast(Origin,
                            shootDirection,
                            out RaycastHit hit,
                            float.MaxValue,
                            ShootConfig.HitMask))
        {
            StartCoroutine(PlayTrail(TrailOrigin, hit.point, hit));

            if (hit.collider.TryGetComponent(out EnemyHealthMulti enemy))
            {
                enemy.TakeDamageRpc(Damage);

            }

        }
        else
        {
                StartCoroutine(PlayTrail(
                TrailOrigin,
                TrailOrigin + (shootDirection * TrailConfig.MissDistance),
                new RaycastHit())
                );
        }
    }

    public Vector3 GetRaycastOrigin()
    {
        Vector3 origin = ShootSystem.transform.position; //si dispara desde el arma

        origin = activeCamera.transform.position +
                 activeCamera.transform.forward * Vector3.Distance(
                    activeCamera.transform.position, ShootSystem.transform.position
                 );
        return origin;
    }
    public Vector3 GetGunForward()
    {
        return Model.transform.forward;
    }

    private void DoProjectileShooting(Vector3 shootDirection)
    {

    }

    public void Reload()
    {
        realoading = true;
        //Invoke("FinishedReload", ReloadTime);
        StartCoroutine(ReloadingCoroutine());
    }
    private IEnumerator ReloadingCoroutine()
    {
        yield return new WaitForSeconds(ReloadTime);
        FinishedReload();
    }
    private void FinishedReload()
    {
        bulletsLeft = MagazineSize;
        realoading = false;
        //Debug.Log("Fin de la recarga");
    }

    private IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit)
    {
        TrailRenderer instance = TrailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = StartPoint;
        yield return null; //Evitar sobreposicion de trails

        instance.emitting = true;

        float distance = Vector3.Distance(StartPoint, EndPoint);
        float remainingDistance = distance;
        while (remainingDistance > 0)
        {
            instance.transform.position = Vector3.Lerp(
                StartPoint,
                EndPoint,
                Mathf.Clamp01(1 - (remainingDistance / distance))
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




    public TrailRenderer CreateTrail()
    {
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

}
