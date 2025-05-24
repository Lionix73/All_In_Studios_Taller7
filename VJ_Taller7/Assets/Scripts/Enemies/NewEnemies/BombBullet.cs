using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    private ThisObjectSounds soundManager;

    private void Awake()
    {
        soundManager = GetComponent<ThisObjectSounds>();
    }

    private bool hasExploded = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        hasExploded = false; // Reset the explosion state
        if (bulletModel != null) bulletModel.enabled = true; // Reset the model visibility
        if (explosionEffect != null) explosionEffect.SetActive(false); // Reset the explosion effect
    }

    public override void Spawn(Vector3 forward, int damage, Transform target)
    {
        this.damage = damage;
        this.target = target;

        if(Rb != null){
            Rb.isKinematic = false;
            Rb.linearVelocity = Vector3.zero;
            Vector3 launchVelocity = CalculateLaunchVelocity(target.position);
            Rb.AddForce(launchVelocity, ForceMode.VelocityChange);
        }
    }

    private Vector3 CalculateLaunchVelocity(Vector3 targetPosition)
    {
        // Direction to target
        Vector3 targetDir = targetPosition - transform.position;
        float distance = targetDir.magnitude;
        
        // Calculate launch angle (45 degrees is optimal for maximum distance)
        float angle = 45f * Mathf.Deg2Rad;
        
        // Calculate initial velocity magnitude needed to reach the target
        float gravity = Physics.gravity.magnitude;
        float velocityMagnitude = Mathf.Sqrt(distance * gravity / Mathf.Sin(2 * angle));
        
        // If we get NaN (target too close or other issues), use a default value
        if (float.IsNaN(velocityMagnitude) || velocityMagnitude <= 0)
        {
            velocityMagnitude = 10f; // Default velocity
        }
        
        // Calculate the launch direction
        Vector3 horizontalDir = new Vector3(targetDir.x, 0, targetDir.z).normalized;
        Vector3 launchDir = horizontalDir * Mathf.Cos(angle) + Vector3.up * Mathf.Sin(angle);
        
        // Return the final velocity vector
        return launchDir * velocityMagnitude;
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Hit");
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

        soundManager.PlaySound("BombExplosion");

        if(explosionEffect != null && bulletModel != null){
            transform.rotation = new Quaternion(0, 0, 0, 0);
            explosionEffect.SetActive(true);
            bulletModel.enabled = false;
        }

        // Track which enemies are damaged to prevent double-counting
        HashSet<IDamageable> damagedEnemies = new HashSet<IDamageable>();


        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, damageableLayer);
        foreach (Collider collider in colliders)
        {
            IDamageable damageable;
            if (collider.TryGetComponent<IDamageable>(out damageable) && !damagedEnemies.Contains(damageable))
            {
                //Debug.Log("BombBullet: Explode() - Damageable hit: " + damageable);
                damagedEnemies.Add(damageable);
                damageable.TakeDamage(damage);
            }
        }
        
        gameObject.GetComponent<Collider>().enabled = false;
        StartCoroutine(WaitForDisable());
    }

    private void OnDrawGizmos()
    {
        if (gameObject.activeSelf)
        {
            Vector3 velocity = CalculateLaunchVelocity(target.position);
            Vector3 position = transform.position;
            
            Gizmos.color = Color.yellow;
            for (int i = 0; i < 50; i++)
            {
                float t = i * 0.1f;
                Vector3 nextPos = position + velocity * t + 0.5f * Physics.gravity * t * t;
                Gizmos.DrawSphere(nextPos, 0.1f);
            }
        }
    }
}
