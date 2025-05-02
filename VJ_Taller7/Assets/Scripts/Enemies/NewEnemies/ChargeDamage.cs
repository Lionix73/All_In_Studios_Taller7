using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ChargeDamage : MonoBehaviour
{
    [SerializeField] private int damage = 20;
    public int Damage { get => damage; set => damage = value; }

    [SerializeField] private float pushForce = 10f;
    [SerializeField] private float upwardForce = 2f;

    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemy == null || enemy.IsDead) return;

        // Apply damage to IDamageable objects
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            Debug.Log($"Enemy {enemy.name} dealt {damage} damage to {other.name}.");
        }

        // Apply force to objects with Rigidbody
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 pushDirection = transform.forward + Vector3.up * upwardForce; // Direction of the charge
            rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
            Debug.Log($"Enemy {enemy.name} pushed {other.name} with force {pushForce}.");
        }
    }
}
