using System.Collections;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour {

    private int objectPenetrated;
    public Rigidbody Rigidbody { get; protected set; }

    [field: SerializeField] 
    public Vector3 SpawnLocation { get; protected set; }

    [SerializeField] private float DelayedDisableTime;

    [SerializeField] private ParticleSystem bulletImpactEffect;

    public delegate void ColisionEvent(Bullet Bullet, Collision collision);
    public event ColisionEvent OnCollision;
     public delegate void EndBulletEvent(Bullet Bullet, Collision collision);
    public event EndBulletEvent OnBulletEnd;


    private void Awake() {
        Rigidbody = GetComponent<Rigidbody>();
    }

    virtual public void Spawn(Vector3 SpawnForce){
        objectPenetrated = 0;
        SpawnLocation = transform.position;
        transform.forward = SpawnForce.normalized;
        Rigidbody.AddForce(SpawnForce);
        //SpawnVelocity = SpawnForce * Time.fixedDeltaTime/Rigidbody.mass;
        StartCoroutine(DestroyBullet(DelayedDisableTime));
    }

    private IEnumerator DestroyBullet(float time){
        yield return null;
        yield return new WaitForSeconds(time);
        OnCollisionEnter(null);
    }
    public virtual void OnCollisionEnter(Collision other) {
        if (bulletImpactEffect!= null){
            ContactPoint contactPoint = other.GetContact(0);

            bulletImpactEffect.transform.forward = contactPoint.normal;
            bulletImpactEffect.Play();
        }

        InvokeCollisionEvent(other);
        objectPenetrated++;
    }

    protected void InvokeCollisionEvent(Collision other){
        OnCollision?.Invoke(this, other);
    }

    protected void IvokeBulletEnd(Collision other){ //para las armas especiales que se destruyen distinto
        OnBulletEnd?.Invoke(this, other);
    }
    private void OnDisable() {
        StopAllCoroutines();
        Rigidbody.linearVelocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
        OnCollision = null;
    }
}