using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class BombBullet : BulletEnemie
{
    [Header("Bomb Settings")]
    [SerializeField] private float explosionRadius = 5f;
    public float ExplosionRadius { get => explosionRadius; set => explosionRadius = value; }
    
    [SerializeField] private LayerMask damageableLayer;
    public LayerMask DamageableLayer { get => damageableLayer; set => damageableLayer = value; }

    [SerializeField] private float upwardForce = 5f;
    public float UpwardForce { get => upwardForce; set => upwardForce = value; }

    [Tooltip("Delay before the bomb explodes after being launched")]
    [SerializeField] private float explosionDelay = 2f;
    public float ExplosionDelay { get => explosionDelay; set => explosionDelay = value; }

    [SerializeField] private GameObject explosionEffect;

    private bool hasExploded = false;

    protected override void OnEnable(){
        base.OnEnable();
    }

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
        Debug.Log("Hit");
        StartCoroutine(DelayedExplosion());
    }

    protected override void OnTriggerEnter(Collider other)
    {
        //Do nothing
    }

    private IEnumerator DelayedExplosion()
    {
        yield return new WaitForSeconds(ExplosionDelay);
        Explode();
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        if(explosionEffect != null && bulletModel != null){
            transform.rotation = new Quaternion(0, 0, 0, 0);
            explosionEffect.SetActive(true);
            bulletModel.enabled = false;
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, damageableLayer);
        foreach (Collider collider in colliders)
        {
            IDamageable damageable;
            if (collider.TryGetComponent<IDamageable>(out damageable))
            {
                Debug.Log("BombBullet: Explode() - Damageable hit: " + damageable);
                damageable.TakeDamage(damage);
            }
        }
        
        StartCoroutine(WaitForDisable());
    }
}
