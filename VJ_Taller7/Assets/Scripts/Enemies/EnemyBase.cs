using UnityEngine;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 100f;
    [SerializeField] protected float health; 
    public float speed = 5f;
    public float attackPower = 10f;

    [SerializeField] protected FloatingHealthBar healthBar;

    [Header("Respawn Settings")]
    [SerializeField] protected bool respawnOnDeath = false;
    [SerializeField] protected float respawnTime = 5f;
    protected Vector3 spawnPoint;

    [Header("Ragdoll Settings")]
    protected Collider mainCollider;
    protected Animator animator;
    protected Collider[] colliders;
    protected Rigidbody[] rigidbodies;

    private void Awake()
    {
        health = maxHealth;
        healthBar = GetComponentInChildren<FloatingHealthBar>();
        healthBar.UpdateHealthBar(health, maxHealth);
    }

    void Start()
    {
        Initialize();
    }

    void Update()
    {
        Move();
    }

    // Method to initialize the enemy
    protected virtual void Initialize()
    {
        DisableRagdoll();
    }

    protected virtual void EnableRagdoll()
    {
        animator.enabled = false;

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
        }
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        mainCollider.enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    protected virtual void DisableRagdoll(){
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
        }
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        mainCollider.enabled = true;
        animator.enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
    }

    // Abstract method to handle animations
    protected abstract void HandleAnims();

    // Abstract method to move the enemy
    protected abstract void Move();

    // Abstract method to handle enemy chasing
    protected abstract void Chase();

    // Method to handle enemy attacks
    public virtual void Attack()
    {

    }

    // Method to handle taking damage
    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        healthBar.UpdateHealthBar(health, maxHealth);

        Debug.Log("Health: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    // Method to handle enemy death
    protected abstract void Die();
}
