using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class Granade : Bullet
{
    [SerializeField] float explotionRadius;
    [SerializeField] int explotionDamage;
    [SerializeField] float curveForce;

    private void Awake() {
        Rigidbody = GetComponent<Rigidbody>();    
    }

    public override void Spawn(Vector3 SpawnForce)
    {
        base.Spawn(SpawnForce);
        Vector3 curveImpulse = new Vector3(0,curveForce,0);
        Rigidbody.AddForce(curveImpulse);
    }

    public override void OnCollisionEnter(Collision other) {
        base.OnCollisionEnter(other);
        Explode();
    }

    private void Explode(){
        Collider[] inRadius = Physics.OverlapSphere(transform.position, explotionRadius);
        foreach (Collider e in inRadius){
            e.TryGetComponent<IDamageable>(out IDamageable enemy);
            if (enemy!=null) enemy.TakeDamage(explotionDamage);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(transform.position,explotionRadius);
    }


}
