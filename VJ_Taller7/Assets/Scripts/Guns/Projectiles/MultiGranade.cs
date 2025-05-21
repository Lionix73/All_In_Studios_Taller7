using Unity.Netcode;
using UnityEngine;

public class MultiGranade : MultiBullet
{
    [SerializeField] float explotionRadius;
    [SerializeField] int explotionDamage;
    [SerializeField] float curveForce;

    private void Awake() {
        Rigidbody = GetComponent<Rigidbody>();    
    }

    public override void Spawn(Vector3 SpawnForce, ulong ownerId)
    {
        base.Spawn(SpawnForce, ownerId);
        Vector3 curveImpulse = new Vector3(0,curveForce,0);
        Rigidbody.AddForce(curveImpulse);
        SpawnRpc(SpawnForce);
    }
    [Rpc(SendTo.Everyone)]
    public void SpawnRpc()
    {
        if (IsServer) return;

        Vector3 curveImpulse = new Vector3(0, curveForce, 0);
        Rigidbody.AddForce(curveImpulse);
    }

    public override void OnCollisionEnter(Collision other) {
        if(!IsServer) return;
        base.OnCollisionEnter(other);
        Explode();
    }

    private void Explode(){
        Collider[] inRadius = Physics.OverlapSphere(transform.position, explotionRadius);
        foreach (Collider e in inRadius){
            e.TryGetComponent<IDamageableMulti>(out IDamageableMulti enemy);
            if (enemy!=null) enemy.TakeDamage(explotionDamage,ownerId);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(transform.position,explotionRadius);
    }


}
