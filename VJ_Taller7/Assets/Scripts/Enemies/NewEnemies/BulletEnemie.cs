using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletEnemie : PoolableObject
{
    [SerializeField] private float autoDestroyTime = 5f;
    public float AutoDestroyTime { get => autoDestroyTime; set => autoDestroyTime = value; }

    [SerializeField] private float moveSpeed = 10f;
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

    [SerializeField] private int damage = 10;
    public int Damage { get => damage; set => damage = value; }

    public Rigidbody Rb { get; private set; }

    private const string DISABLE_METHOD_NAME = "Disable";

    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        CancelInvoke(DISABLE_METHOD_NAME);
        Invoke(DISABLE_METHOD_NAME, autoDestroyTime);
    }

    private void OggerEnter(Collider other)
    {
        IDamageable damageable;

        if (other.TryGetComponent<IDamageable>(out damageable))
        {
            damageable.TakeDamage(damage);
            Disable();
        }
    }

    private void Disable(){
        CancelInvoke(DISABLE_METHOD_NAME);
        Rb.linearVelocity = Vector3.zero;
        gameObject.SetActive(false);
    }
}
