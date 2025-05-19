using UnityEngine;

public class MultiPiercingBullet : MultiBullet
{
    [SerializeField] private int baseDamage;
    private int damage;
    [SerializeField] private int damageReductionPerPierce;

    //[SerializeField] private Collider physicColl;

    private void Awake() {
        Rigidbody = GetComponent<Rigidbody>();
        //physicColl.enabled = false;
    }
    public override void Spawn(Vector3 SpawnForce, ulong ownerId)
    {
        base.Spawn(SpawnForce, ownerId);
        damage = baseDamage;
        
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log(other.gameObject.name);
        GetComponent<Collider>().isTrigger=false;
    }

    public override void OnCollisionEnter(Collision other) {
        if (other!= null) Debug.Log("Colision with " + other.gameObject.name);
        base.OnCollisionEnter(other);

    }
}
