using UnityEngine;
using System.Collections;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class MultiBulletEnemy : NetworkBehaviour
{

    [Header("Bullet Settings")]
    [Tooltip("Tiempo en segundos que el bullet se destruye automaticamente si no colisiona con nada")]
    [SerializeField] protected float autoDestroyTime = 5f;
    public float AutoDestroyTime { get => autoDestroyTime; set => autoDestroyTime = value; }

    [SerializeField] protected float moveSpeed = 10f;
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

    [SerializeField] protected int damage = 10;
    public int Damage { get => damage; set => damage = value; }

    [Tooltip("Tiempo en segundos que el bullet espera para desactivarse tras colisionar con algo")]
    [SerializeField] protected float waitForDisable = 3f;


    [Header("Effects")]
    [SerializeField] private GameObject bulletCollisionEffect;

    [SerializeField] protected MeshRenderer bulletModel;

    [SerializeField] Rigidbody rb;

    public Rigidbody Rb { get => rb; private set => rb = value; }

    protected Transform target;

    protected const string DISABLE_METHOD_NAME = "Disable";

    public delegate void ColisionEvent(MultiBulletEnemy Bullet);
    public event ColisionEvent OnCollision;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

       // Rb = GetComponent<Rigidbody>();
    }

    

    protected virtual void OnEnable()
    {
            //Debug.Log("BulletEnemyOnEnable");
            Rb.linearVelocity = Vector3.zero;
            //CancelInvoke(DISABLE_METHOD_NAME);

    }

    public virtual void Spawn(Vector3 forward, int damage, Transform target, Vector3 bulletPos, Quaternion bulletRot)
    {
        Invoke(DISABLE_METHOD_NAME, autoDestroyTime);

        this.damage = damage;
        this.target = target;
        transform.position = bulletPos;
        transform.rotation = bulletRot;
        Rb.AddForce(forward * moveSpeed, ForceMode.VelocityChange);
        SpawnRpc(forward, bulletPos, bulletRot);
    }

    [Rpc(SendTo.Everyone)]
    public void SpawnRpc(Vector3 forward, Vector3 bulletPos, Quaternion bulletRot)
    {
        if (IsServer) return;
        transform.position = bulletPos;
        transform.rotation = bulletRot;
        Rb.AddForce(forward * moveSpeed, ForceMode.VelocityChange);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if(!IsServer) return;
        IDamageableMulti damageable;
        
        //StartCoroutine(WaitForDisable());

        if (bulletCollisionEffect != null){
            bulletCollisionEffect.SetActive(true);
        }

        if (other.TryGetComponent<IDamageableMulti>(out damageable))
        {
            // SONIDO golpeo Jugador/emigo, cosa damageable
            damageable.TakeDamage(damage, 10);
        }
        else
        {
            // SONIDO golpeo una superficie
        }

        //if (bulletModel!=null) bulletModel.enabled = false;
        //gameObject.GetComponent<Collider>().enabled = false;
    }

    protected virtual IEnumerator WaitForDisable(MultiBulletEnemy bullet){
        yield return new WaitForSeconds(waitForDisable);
        Disable(this);
    }

    protected void Disable(MultiBulletEnemy bullet){

        Debug.Log("Disable");
        //CancelInvoke(DISABLE_METHOD_NAME);

        OnCollision?.Invoke(bullet);
        //Rb.linearVelocity = Vector3.zero;
        //gameObject.SetActive(false);


    }
}