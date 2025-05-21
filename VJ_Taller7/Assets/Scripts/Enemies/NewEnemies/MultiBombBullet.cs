using UnityEngine;
using System.Collections;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class MultiBombBullet : MultiBulletEnemy
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

    [SerializeField] private ParticleSystem explosionEffect;

    private ThisObjectSounds soundManager;

    public delegate void ExplosionEvent(MultiBombBullet Bullet);
    public event ExplosionEvent OnExplosion;

    private void Awake()
    {
        soundManager = GetComponent<ThisObjectSounds>();
    }

    private bool hasExploded = false;

    protected override void OnEnable(){
        base.OnEnable();
        //if (bulletModel != null) bulletModel.enabled = true; // Reset the model visibility
        //if (explosionEffect != null)
    }

    public override void Spawn(Vector3 forward, int damage, Transform target, Vector3 bombPos, Quaternion bombRot)
    {
        this.damage = damage;
        this.target = target;
        transform.localPosition = bombPos;
        transform.rotation = bombRot;
        hasExploded = false; // Reset the explosion state
        gameObject.GetComponent<Collider>().enabled = true;
        explosionEffect.gameObject.SetActive(false); // Reset the explosion effect
        if (Rb != null)
        {
            Rb.linearVelocity = CalculateLaunchVelocity(target.position);
        }
        SpawnRpc(forward, target.position,bombPos, bombRot);
    }

    [Rpc(SendTo.Everyone)]
    public void SpawnRpc(Vector3 forward,Vector3 targetPos ,Vector3 bulletPos, Quaternion bulletRot)
    {
        if (IsServer) return;
        gameObject.GetComponent<Collider>().enabled = true ;
        explosionEffect.gameObject.SetActive(false); // Reset the explosion effect
        transform.position = bulletPos;
        transform.rotation = bulletRot;
        Rb.linearVelocity = CalculateLaunchVelocity(targetPos);
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
        if(!IsServer) return;
        Debug.Log("Hit");
        DelayedExplosionVoid();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        //Do nothing
    }
    private void DelayedExplosionVoid()
    {
        if (!IsServer) return;
        StartCoroutine(DelayedExplosion());
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

        // Track which enemies are damaged to prevent double-counting
        HashSet<IDamageableMulti> damagedEnemies = new HashSet<IDamageableMulti>();


        /*Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, damageableLayer);
        foreach (Collider collider in colliders)
        {
            IDamageableMulti damageable;
            if (collider.TryGetComponent<IDamageableMulti>(out damageable) && !damagedEnemies.Contains(damageable))
            {
                //Debug.Log("BombBullet: Explode() - Damageable hit: " + damageable);
                damagedEnemies.Add(damageable);
                damageable.TakeDamage(damage,10);
            }
        }*/

        Collider[] inRadius = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider e in inRadius)
        {
            e.TryGetComponent<IDamageableMulti>(out IDamageableMulti enemy);
            if (enemy != null) enemy.TakeDamage(damage, 10);
        }
        gameObject.GetComponent<Collider>().enabled = false;
        //OnExplosion?.Invoke(this);
        soundManager.PlaySound("BombExplosion");

        transform.rotation = new Quaternion(0, 0, 0, 0);
        explosionEffect.gameObject.SetActive(true);
        if (explosionEffect != null && bulletModel != null)
        {

            //bulletModel.enabled = false;
        }
        ExplosionEffectRpc();

        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(WaitForDisable());
    }


    [Rpc(SendTo.Everyone)]
    public void ExplosionEffectRpc()
    {
        if (IsServer) return;
        soundManager.PlaySound("BombExplosion");

        if (explosionEffect != null && bulletModel != null)
        {
            gameObject.GetComponent<Collider>().enabled = false;
            transform.rotation = new Quaternion(0, 0, 0, 0);
            explosionEffect.gameObject.SetActive(true);
            //explosionEffect.Play();
            //bulletModel.enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
