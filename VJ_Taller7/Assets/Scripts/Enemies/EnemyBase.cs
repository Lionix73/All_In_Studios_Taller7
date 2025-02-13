using UnityEngine;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 100f;
    [SerializeField] protected float health; 
    public float speed = 5f;
    public float attackPower = 10f;

    [SerializeField] FloatingHealthBar healthBar;

    [Header("Respawn Settings")]
    [SerializeField] private bool respawnOnDeath = false;
    [SerializeField] private float respawnTime = 5f;

    private Vector3 initialPosition;

    private void Awake()
    {
        health = maxHealth;
        healthBar = GetComponentInChildren<FloatingHealthBar>();
        healthBar.UpdateHealthBar(health, maxHealth);
        initialPosition = transform.position;
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

    }

    // Abstract method to handle animations
    protected abstract void HandleAnims();

    // Abstract method to move the enemy
    protected abstract void Move();

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
    protected virtual void Die()
    {
        if (respawnOnDeath)
        {
            StartCoroutine(Respawn());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to handle enemy respawning
    private IEnumerator Respawn()
    {
        gameObject.SetActive(false);
        yield return new WaitForSeconds(respawnTime);
        transform.position = initialPosition;
        health = maxHealth;
        healthBar.UpdateHealthBar(health, maxHealth);
        gameObject.SetActive(true);
    }
}
