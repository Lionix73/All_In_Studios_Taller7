using System.Dynamic;
using Unity.VisualScripting;
using UnityEngine;

public class GoldenFeathers : Bullet
{
    [field: SerializeField] 
    public Transform whereToReturn {get ; private set;}
    public bool isReturning = false;
    [SerializeField] private float returningStrength;
    //private PhysicsMaterial physics_Mat;

    private void Awake() {
        Rigidbody = GetComponent<Rigidbody>();
    }

    override public void Spawn(Vector3 SpawnForce){
        SpawnLocation = transform.position;
        transform.forward = SpawnForce.normalized;
        Rigidbody.AddForce(SpawnForce);
        isReturning = false;

        //Agregar al evento de recarga del gun manager el return()
        GameManager.Instance.gunManager.ReloadEvent += Return;
    }

    public void Return(){
        whereToReturn = GameManager.Instance.playerManager.gunManager.CurrentGun.ShootSystem.transform;

        Vector3 direction = whereToReturn.position + - transform.position;
        transform.forward = direction.normalized;
        Rigidbody.AddForce(direction * returningStrength);
        isReturning = true;
    }

    public override void OnCollisionEnter(Collision other) {
        
        if(other.gameObject.layer == LayerMask.NameToLayer("Enemy")){
           InvokeCollisionEvent(other);
        }

        if (isReturning)
        {
            IvokeBulletEnd(other);
            //Liberar de la pool... supongo que con otro evento
        }

        //StopAllCoroutines();
        Rigidbody.linearVelocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
        Rigidbody.useGravity = false;
        
    }

    private void OnCollisionExit(Collision other) {
        if (isReturning){
            if(other.gameObject.layer == LayerMask.NameToLayer("Enemy")){
                InvokeCollisionEvent(other);
            }
        }
    }
}
