using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletEnemie : PoolableObject
{
    [SerializeField] protected float autoDestroyTime = 5f;
    public float AutoDestroyTime { get => autoDestroyTime; set => autoDestroyTime = value; }

    [SerializeField] protected float moveSpeed = 10f;
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

    [SerializeField] protected int damage = 10;
    public int Damage { get => damage; set => damage = value; }

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

        if (other.TryGetComponent<IDamageable>(out damageable))
        {
            damageable.TakeDamage(damage);
        }

        Disable();
    }

    protected void Disable(){
        //Debug.Log("Disable");
        CancelInvoke(DISABLE_METHOD_NAME);
        Rb.linearVelocity = Vector3.zero;
        gameObject.SetActive(false);
    }
}
