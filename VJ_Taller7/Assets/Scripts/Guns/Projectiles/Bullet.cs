using System.Collections;
using Unity.IO.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour {

    private int objectPenetrated;
    public Rigidbody Rigidbody { get; protected set; }

    [field: SerializeField] 
    public Vector3 SpawnLocation { get; protected set; }

    [SerializeField] private float DelayedDisableTime;

    [SerializeField] private GameObject bulletImpactEffect;

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

    protected IEnumerator DestroyBullet(float time){
        yield return null;
        yield return new WaitForSeconds(time);
        OnCollisionEnter(null);
    }
    public virtual void OnCollisionEnter(Collision other) {
        ImpactEffect(other);

        InvokeCollisionEvent(other);
        objectPenetrated++;
    }
    protected void ImpactEffect(Collision collisionPoint){
        if (bulletImpactEffect!= null){
            ContactPoint contactPoint = collisionPoint.GetContact(0);

            //bulletImpactEffect.transform.forward = contactPoint.normal;
            //bulletImpactEffect.Play();

            GameObject impact = Instantiate(bulletImpactEffect, contactPoint.point, quaternion.identity);
            impact.transform.up = contactPoint.normal;
            Destroy(impact, 0.5f);
        }
    }

    protected void InvokeCollisionEvent(Collision other){
        OnCollision?.Invoke(this, other);
    }

    protected void InvokeBulletEnd(Collision other){ //para las armas especiales que se destruyen distinto
        OnBulletEnd?.Invoke(this, other);
    }
    private void OnDisable() {
        StopAllCoroutines();
        Rigidbody.linearVelocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
        OnCollision = null;
    }
}