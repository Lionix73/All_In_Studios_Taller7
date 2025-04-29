using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class BulletEnemie : PoolableObject
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

    public Rigidbody Rb { get; private set; }

    protected Transform target;

    protected const string DISABLE_METHOD_NAME = "Disable";


    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
    }

    protected virtual void OnEnable()
    {
        CancelInvoke(DISABLE_METHOD_NAME);
        Invoke(DISABLE_METHOD_NAME, autoDestroyTime);
    }

    public virtual void Spawn(Vector3 forward, int damage, Transform target)
    {
        this.damage = damage;
        this.target = target;
        Rb.AddForce(forward * moveSpeed, ForceMode.VelocityChange);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        IDamageable damageable;

        if(bulletCollisionEffect != null){
            bulletCollisionEffect.SetActive(true);
        }

        if (other.TryGetComponent<IDamageable>(out damageable))
        {
            // SONIDO golpeo Jugador/emigo, cosa damageable
            damageable.TakeDamage(damage);
        }
        else
        {
            // SONIDO golpeo una superficie
        }

        if (bulletModel!=null) bulletModel.enabled = false;
        gameObject.GetComponent<Collider>().enabled = false;
        StartCoroutine(WaitForDisable());
    }

    protected virtual IEnumerator WaitForDisable(){
        yield return new WaitForSeconds(waitForDisable);
        Disable();
    }

    protected void Disable(){
        //Debug.Log("Disable");
        CancelInvoke(DISABLE_METHOD_NAME);
        Rb.linearVelocity = Vector3.zero;
        gameObject.SetActive(false);
    }

    public override void OnDisable(){
        base.OnDisable();
    }
}
