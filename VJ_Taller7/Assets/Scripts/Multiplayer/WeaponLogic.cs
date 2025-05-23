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
    public int Damage;
    public int MagazineSize;
    public float ReloadTime;

    public ShootConfigScriptableObjtect ShootConfig;
    public TrailConfigScriptableObject TrailConfig;
    public Sprite crosshairImage;

    public MonoBehaviour ActiveMonoBehaviour;
    public GameObject Model;
    public Camera activeCamera;
    public float LastShootTime;
    private NetworkVariable<int> bulletsLeft = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);
    public int BulletsLeft
    {
        get => bulletsLeft.Value;
        set
        {
            if (IsServer)
                bulletsLeft.Value = value;
        }
    }
    public bool realoading;
    public bool Realoading
    {
        get => realoading;
    }
    public ParticleSystem ShootSystem;
    public ObjectPool<TrailRenderer> TrailPool;
    public ObjectPool<MultiBullet> BulletPool;
    public ObjectPool<MultiBullet> GFeatherPool;
    public ObjectPool<MultiBullet> SFeatherPool;

    public delegate void EmptyAmmo();
    public event EmptyAmmo OnEmptyAmmo;


    private NetworkObjectPool networkObjectPool;

    public bool noFriendsInWar;

    public void SetBullets(int amount)
    {
        if (IsServer)
        {
            bulletsLeft.Value = amount;
        }
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();



        networkObjectPool = NetworkObjectPool.Singleton.GetComponent<NetworkObjectPool>();

        activeCamera = Camera.main;

        Model = gameObject;
        BulletsLeft = currentGun.bulletsLeft;
        MagazineSize = currentGun.MagazineSize;

        Type = currentGun.Type;
        name = currentGun.Name;
        UIImage = currentGun.UIImage;
        Damage = currentGun.Damage;
        ReloadTime = currentGun.ReloadTime;

        ShootConfig = currentGun.ShootConfig;
        TrailConfig = currentGun.TrailConfig;
        crosshairImage = currentGun.CrosshairImage;

        //ActiveMonoBehaviour = currentGun.ActiveMonoBehaviour; //Este es el que entiendo que debe dar porblemas
        LastShootTime = currentGun.LastShootTime;

        realoading = currentGun.Realoading;
        //TrailPool = currentGun.TrailPool;
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);


        ShootSystem = Model.GetComponentInChildren<ParticleSystem>();

        /* if (IsServer)
         {
             if (!ShootConfig.IsHitScan)
             { BulletPool = new ObjectPool<MultiBullet>(CreateBullet); }
             if (ShootConfig.ShootingType == ShootType.Special)
             {
                 GFeatherPool = new ObjectPool<MultiBullet>(CreateBullet);
                 SFeatherPool = new ObjectPool<MultiBullet>(CreateBullet);
             }

         }*/

    }



    public void Shoot(ulong ownerClientId)
    {
        ShootServerRpc(ownerClientId);
    }
    [Rpc(SendTo.Server)]
    public void ShootServerRpc(ulong ownerClientId)
    {
        if (Time.time > ShootConfig.FireRate + LastShootTime && BulletsLeft > 0 && !realoading)
        {
            LastShootTime = Time.time;
            ShootVFXRpc();
            BulletsLeft -= ShootConfig.BulletsPerShot;
            if (BulletsLeft <= 0)
            {
                OnEmptyAmmo?.Invoke();
            }

            for (int i = 0; i < ShootConfig.BulletsPerShot; i++)
            {
                Debug.Log(ShootConfig.BulletsPerShot);
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
                    DoHitScanShooting(shootDirection, ShootSystem.transform.position, ShootSystem.transform.position, ownerClientId);
                }
                else
                {
                    switch (ShootConfig.ShootingType)
                    {
                        case ShootType.Projectile:
                            DoProjectileShooting(shootDirection, ownerClientId);
                            return;
                        case ShootType.Special:
                            DoSpecialShooting(shootDirection, ownerClientId);
                            return;
                    }
                }

            }
        }
    }

    private void DoHitScanShooting(Vector3 shootDirection, Vector3 Origin, Vector3 TrailOrigin, ulong ownerClientId, int Iteration = 0)
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
            else if (hit.collider.TryGetComponent(out IDamageableMulti enemyDmg))
            {
                enemyDmg.TakeDamage(Damage, ownerClientId); //simplemente saber si se puede hacer da�o, me falta por ver si espec�ficar los cr�ticos
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

    private void DoProjectileShooting(Vector3 ShootDirection, ulong ownerClientId)
    {
        NetworkObject netObject = networkObjectPool.GetNetworkObject(ShootConfig.BulletPrefabMulti.gameObject, ShootDirection, Quaternion.Euler(0, 0, 0));
        if (!netObject.IsSpawned) netObject.Spawn();
        MultiBullet bullet = netObject.GetComponent<MultiBullet>();
        //MultiBullet bullet = BulletPool.Get();
        // bullet.gameObject.SetActive(true);
        //bullet.Spawn(ShootDirection * ShootConfig.BulletSpawnForce);
        GetBulletRpc(netObject.NetworkObjectId, ShootDirection, ownerClientId);
        bullet.OnCollision += HandleBulletCollision;


        // El trail de la bala podemos decidir si ponerlo en la bala, o usar el mismo pool que con el hitscan

    }
    [Rpc(SendTo.Everyone)]
    public void GetBulletRpc(ulong modelNetworkObjectId, Vector3 shootDirection, ulong ownerClientId)
    {
        // Obt�n el NetworkObject correspondiente al ID
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(modelNetworkObjectId, out NetworkObject spawnBullet))
        {
            if (IsClient) spawnBullet.gameObject.SetActive(true);
            MultiBullet bullet = spawnBullet.GetComponent<MultiBullet>();
            bullet.transform.position = ShootSystem.transform.position;
            bullet.Initialize(ownerClientId);

            if (!IsServer) return;
            //bullet.Initialize(ownerClientId);
            bullet.Spawn(shootDirection * ShootConfig.BulletSpawnForce, ownerClientId);
        }
        else
        {
            Debug.LogError("Failed to find NetworkObject with ID: " + modelNetworkObjectId);
        }

    }

    private void DoSpecialShooting(Vector3 ShootDirection, ulong ownerClientId)
    {
        switch (Type)
        {
            case GunType.MysticCanon:
                return;
            case GunType.GoldenFeather:
                //MultiBullet feather = GFeatherPool.Get();
                //feather.gameObject.SetActive(true);
                NetworkObject netObjectGF = networkObjectPool.GetNetworkObject(ShootConfig.BulletPrefabMulti.gameObject, ShootDirection, Quaternion.Euler(0, 0, 0));
                if (!netObjectGF.IsSpawned) netObjectGF.Spawn();
                MultiBullet feather = netObjectGF.GetComponent<MultiBullet>();
                GetBulletRpc(netObjectGF.NetworkObjectId, ShootDirection,ownerClientId);
                feather.OnCollision += HandleBulletCollision;
                feather.OnBulletEnd += HandleGoldenBulletCollision; //Para poder eliminar tambien las balas especiales 
                //feather.transform.position = ShootSystem.transform.position;
                //feather.Spawn(ShootDirection * ShootConfig.BulletSpawnForce);
                return;
            case GunType.ShinelessFeather:
                //MultiBullet shineless = SFeatherPool.Get();
                NetworkObject netObjectSF = networkObjectPool.GetNetworkObject(ShootConfig.BulletPrefabMulti.gameObject, ShootDirection, Quaternion.Euler(0, 0, 0));
                if (!netObjectSF.IsSpawned) netObjectSF.Spawn();
                MultiBullet shineless = netObjectSF.GetComponent<MultiBullet>();
                GetBulletRpc(netObjectSF.NetworkObjectId, ShootDirection, ownerClientId);
                //shineless.gameObject.SetActive(true);
                shineless.OnCollision += HandleBulletCollision;
                shineless.OnBulletEnd += HandleShinelessFeather; //Para poder eliminar tambien las balas especiales 
                //shineless.transform.position = ShootSystem.transform.position;
                //shineless.Spawn(ShootDirection * ShootConfig.BulletSpawnForce);
                return;
        }
    }

    public void Reload()
    {
        ReloadRpc();
    }
    [Rpc(SendTo.Server)]
    public void ReloadRpc()
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
        BulletsLeft = MagazineSize;
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



    private void HandleBulletCollision(MultiBullet bullet, Collision collision)
    {
        if (!IsServer) return;
        // En caso de usar la pool de trail, hay que desactivarla desde aqui
        if (ShootConfig.ShootingType != ShootType.Special)
        {
            //bullet.gameObject.SetActive(false);
            NetworkObject netObj = bullet.gameObject.GetComponent<NetworkObject>();
            networkObjectPool.ReturnNetworkObject(netObj, ShootConfig.BulletPrefabMulti.gameObject);
            ReturnBulletRpc(netObj.NetworkObjectId);
        }

        if (collision != null)
        {
            ContactPoint contactPoint = collision.GetContact(0);

            Collider colliderHit = contactPoint.otherCollider;
            if (colliderHit == null || colliderHit.gameObject.layer != LayerMask.NameToLayer("Enemy"))
            {
                return;
            }

            //if (colliderHit.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;

            if (colliderHit.TryGetComponent(out IDamageableMulti enemy))
            {
                enemy.TakeDamage(Damage, OwnerClientId);
            }
            else if (colliderHit.TryGetComponent(out EnemyHealthMulti enemyM))
            {
                enemyM.TakeDamageRpc(Damage);
            }
        }



    }
    private void HandleGoldenBulletCollision(MultiBullet bullet, Collision collision)
    {
        NetworkObject netObj = bullet.gameObject.GetComponent<NetworkObject>();
        networkObjectPool.ReturnNetworkObject(netObj, ShootConfig.BulletPrefabMulti.gameObject);
        ReturnBulletRpc(netObj.NetworkObjectId);
        bullet.OnCollision -= HandleBulletCollision;
        bullet.OnBulletEnd -= HandleGoldenBulletCollision; //Para poder eliminar tambien las balas especiales 

        //condicion de la shineless feather para que no recargue
    }

    private void HandleShinelessFeather(MultiBullet bullet, Collision collision)
    {
        NetworkObject netObj = bullet.gameObject.GetComponent<NetworkObject>();
        networkObjectPool.ReturnNetworkObject(netObj, ShootConfig.BulletPrefabMulti.gameObject);
        ReturnBulletRpc(netObj.NetworkObjectId);
        BulletsLeft = 1;
        bullet.OnCollision -= HandleBulletCollision;
        bullet.OnBulletEnd -= HandleShinelessFeather;
    }

    private MultiBullet CreateBullet()
    {
        NetworkObject bulletObj = networkObjectPool.GetNetworkObject(ShootConfig.BulletPrefabMulti.gameObject,Vector3.zero,Quaternion.Euler(0f, 0f, 0f));
        return bulletObj.GetComponent<MultiBullet>();
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

    [Rpc(SendTo.Everyone)]
    public void ShootVFXRpc()
    {
        ShootSystem.Play();
    }
    [Rpc(SendTo.Everyone)]
    public void ReturnBulletRpc(ulong modelNetworkObjectId) 
    {
        if (!IsClient) return;

        // Obt�n el NetworkObject correspondiente al ID
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(modelNetworkObjectId, out NetworkObject spawnBullet))
        {
            //spawnBullet.Despawn();
            spawnBullet.gameObject.SetActive(false);

        }
        else
        {
            Debug.LogError("Failed to find NetworkObject with ID: " + modelNetworkObjectId);
        }
    }
}
