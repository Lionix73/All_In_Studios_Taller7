using FMODUnity;
using System.Dynamic;
using Unity.VisualScripting;
using UnityEngine;

public class GoldenFeathers : Bullet
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

    override public void Spawn(Vector3 SpawnForce){
        SpawnLocation = transform.position;
        transform.forward = SpawnForce.normalized;
        Rigidbody.AddForce(SpawnForce);
        isReturning = false;
        isFlying = true;

        //Agregar al evento de recarga del gun manager el return()
        GameManager.Instance.gunManager.ReloadEvent += Return;
    }

    public virtual void Return(){
        whereToReturn = GameManager.Instance.playerManager.gunManager.CurrentGun.ShootSystem.transform;

        Vector3 direction = whereToReturn.position + - transform.position;
        transform.forward = direction.normalized;
        Rigidbody.AddForce(direction * returningStrength);
        isReturning = true;
        isFlying = true;

        soundManager.PlaySound("featherReturn");
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
            if(other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                InvokeCollisionEvent(other);
            }
        }
    }
}
