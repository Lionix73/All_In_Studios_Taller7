using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour {

    private int objectPenetrated;
    public Rigidbody Rigidbody { get; private set; }

    [field: SerializeField] public Vector3 SpawnVelocity { get; private set; }
    [field: SerializeField] public Vector3 SpawnLocation { get; private set; }

    public delegate void ColisionEvent(Collision collision);
    public event ColisionEvent OnCollision;

    private void Awake() {
        Rigidbody = GetComponent<Rigidbody>();
    }

    public void Spawn(Vector3 SpawnForce){
        objectPenetrated = 0;
        SpawnLocation = transform.position;
        transform.forward = SpawnForce.normalized;
        Rigidbody.AddForce(SpawnForce);
        SpawnVelocity = SpawnForce * Time.fixedDeltaTime/Rigidbody.mass;
        StartCoroutine(DestroyBullet(2));
    }

    private IEnumerator DestroyBullet(float time){
        yield return null;
        yield return new WaitForSeconds(time);
        OnCollisionEnter(null);
    }
    private void OnCollisionEnter(Collision other) {
        //OnCollision?.Invoke(this, other, objectPenetrated); //Por algun motico no funciona
        objectPenetrated++;
    }
    private void OnDisable() {
        StopAllCoroutines();
        Rigidbody.linearVelocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
        OnCollision = null;
    }
}