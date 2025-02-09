using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public float maxHealth = 100f;
    protected float health; 
    public float speed = 5f;
    public float attackPower = 10f;

    [SerializeField] FloatingHealthBar healthBar;

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

    }

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
        Destroy(gameObject);
    }
}
