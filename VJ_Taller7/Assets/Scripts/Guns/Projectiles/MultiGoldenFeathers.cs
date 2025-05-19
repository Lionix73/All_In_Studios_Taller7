using Unity.Netcode;
using UnityEngine;

public class MultiGoldenFeathers : MultiBullet
{
    [field: SerializeField] 
    public Transform whereToReturn {get ; private set;}
    public bool isReturning = false;
    public bool isFlying;
    [SerializeField] private float returningStrength;
    //private PhysicsMaterial physics_Mat;


    protected ThisObjectSounds soundManager;

    private void Awake() {
        Rigidbody = GetComponent<Rigidbody>();
        soundManager = GetComponent<ThisObjectSounds>();
    }

    override public void Spawn(Vector3 SpawnForce, ulong ownerId){
        if (!IsServer) return;

        SpawnLocation = transform.position;
        transform.forward = SpawnForce.normalized;
        Rigidbody.AddForce(SpawnForce);
        isReturning = false;
        isFlying = true;
        //Agregar al evento de recarga del gun manager el return()
        GunManagerMulti2 gunManager;
        gunManager = MultiPlayerManager.Instance.GetPlayerGunManager(ownerId);
        gunManager.ReloadEvent += Return;
        SpawnClientRpc(SpawnForce, ownerId);


    }
    [Rpc(SendTo.Everyone)]
    private void SpawnClientRpc(Vector3 SpawnForce, ulong ownerId)
    {
        if (IsServer) return; // Solo se ejecuta en clientes

        SpawnLocation = transform.position;
        transform.forward = SpawnForce.normalized;
        Rigidbody.AddForce(SpawnForce);
        isReturning = false;
        isFlying = true;
    }

    public virtual void Return(ulong ownerId)
    {
        if (!IsServer) return;

        GunManagerMulti2 gunManager = MultiPlayerManager.Instance.GetPlayerGunManager(ownerId);
        whereToReturn = gunManager.transform;

        Vector3 direction = whereToReturn.position - transform.position;
        transform.forward = direction.normalized;
        Rigidbody.AddForce(direction * returningStrength);
        isReturning = true;
        isFlying = true;

        // Sincronizar con clientes
        ReturnClientRpc(whereToReturn.position, returningStrength);

        soundManager.PlaySound("featherReturn");
    }

    [Rpc(SendTo.Everyone)]
    private void ReturnClientRpc(Vector3 gunManagerPosition, float returningStrength)
    {
        if (IsServer) return;


        Vector3 direction = gunManagerPosition - transform.position;
        transform.forward = direction.normalized;
        Rigidbody.AddForce(direction * returningStrength);
        isReturning = true;
        isFlying = true;
    }

    public override void OnCollisionEnter(Collision other) {
        if(!isFlying) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            InvokeCollisionEvent(other);
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            soundManager.PlaySound("featherHitEnemy");
            InvokeCollisionEvent(other);
        }
        else //Si no golpea un enemigo que haga el efecto
        {
            soundManager.PlaySound("featherHitSurface");
            ImpactEffect(other);
        }

        if (isReturning)
        {
            InvokeBulletEnd(other);
            //Liberar de la pool... supongo que con otro evento
        }

        //StopAllCoroutines();
        Rigidbody.linearVelocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
        Rigidbody.useGravity = false;
    }

    public virtual void OnCollisionExit(Collision other) {
        if(!isFlying) return;

        if (isReturning){
            if(other.gameObject.layer == LayerMask.NameToLayer("Enemy")){
                InvokeCollisionEvent(other);
            }
        }
    }
}
