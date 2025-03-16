using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class BombBullet : BulletEnemie
{
    [SerializeField] private float explosionRadius = 5f;
    public float ExplosionRadius { get => explosionRadius; set => explosionRadius = value; }
    
    [SerializeField] private LayerMask damageableLayer;
    public LayerMask DamageableLayer { get => damageableLayer; set => damageableLayer = value; }

    [SerializeField] private float upwardForce = 5f;
    public float UpwardForce { get => upwardForce; set => upwardForce = value; }

    [SerializeField] private float explosionDelay = 2f;
    public float ExplosionDelay { get => explosionDelay; set => explosionDelay = value; }

    public override void Spawn(Vector3 forward, int damage, Transform target)
    {
        this.damage = damage;
        this.target = target;
        Rb.linearVelocity = CalculateLaunchVelocity(target.position);
    }

    private Vector3 CalculateLaunchVelocity(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        float heightDifference = direction.y;
        direction.y = 0;
        float distance = direction.magnitude;
        direction.y = distance;
        distance += heightDifference;

        float angle = 45f * Mathf.Deg2Rad;
        float velocity = Mathf.Sqrt(distance * Physics.gravity.magnitude / Mathf.Sin(2 * angle));

        if (float.IsNaN(velocity))
        {
            velocity = 0;
        }

        return velocity * direction.normalized + Vector3.up * upwardForce;
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        StartCoroutine(DelayedExplosion());
    }

    protected override void OnTriggerEnter(Collider other)
    {
        //Do nothing
    }

    private IEnumerator DelayedExplosion()
    {
        yield return new WaitForSeconds(explosionDelay);
        Explode();
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, damageableLayer);
        foreach (Collider collider in colliders)
        {
            IDamageable damageable;
            if (collider.TryGetComponent<IDamageable>(out damageable))
            {
                damageable.TakeDamage(damage);
            }
        }
        Disable();
    }

    protected new void Disable(){
        CancelInvoke(DISABLE_METHOD_NAME);
        Rb.linearVelocity = Vector3.zero;
        gameObject.SetActive(false);
    }
}
