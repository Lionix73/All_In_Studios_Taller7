using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunScriptableObject : ScriptableObject {
    public GunType Type;
    public string Name;
    public GameObject ModelPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;

    public ShootConfigScriptableObjtect ShootConfig;
    public TrailConfigScriptableObject TrailConfig;

    //De aqui abajo en revision
    private MonoBehaviour ActiveMonoBehaviour;
    private GameObject Model;
    private float LastShootTime;
    private ParticleSystem ShootSystem;
    private ObjectPool<TrailRenderer> TrailPool;


    public void Spawn(Transform Parent, MonoBehaviour ActiveMonoBehaviour) {
        this.ActiveMonoBehaviour = ActiveMonoBehaviour;
        LastShootTime = 0f;
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);

        Model = Instantiate(ModelPrefab);
        Model.transform.SetParent(Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localEulerAngles = SpawnRotation;
        ShootSystem = Model.GetComponentInChildren<ParticleSystem>();
    }

    public void Shoot(){
        if (Time.time > ShootConfig.FireRate + LastShootTime) {
            LastShootTime = Time.time;
            ShootSystem.Play();
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

            if (Physics.Raycast(ShootSystem.transform.position, 
                                shootDirection, 
                                out RaycastHit hit, 
                                float.MaxValue, 
                                ShootConfig.HitMask))
                {
                ActiveMonoBehaviour.StartCoroutine(PlayTrail(ShootSystem.transform.position, hit.point, hit));
                if (hit.collider.TryGetComponent(out EnemyBase enemey)){
                    enemey.TakeDamage(20);
                }
            }
            else {
                ActiveMonoBehaviour.StartCoroutine(PlayTrail(
                    ShootSystem.transform.position,
                    ShootSystem.transform.position + (shootDirection * TrailConfig.MissDistance),
                    new RaycastHit())
                    );
            }
        }
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




    private TrailRenderer CreateTrail() {
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