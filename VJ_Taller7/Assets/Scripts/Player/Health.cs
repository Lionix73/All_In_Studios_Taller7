using TMPro;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private TextMeshProUGUI healthDisplay;
    [SerializeField] private float currentHealth = 20f;
    [SerializeField] private float maxHealth = 100f;
    private void Awake()
    {
        //UIManager.Singleton.GetPlayerHealth(currentHealth, maxHealth);
    }
    void Update()
    {
        UIManager.Singleton.GetPlayerHealth(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        //UIManager.Singleton.GetPlayerHealth(currentHealth, maxHealth);
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public float GetHealth
    {
        get { return currentHealth; }

        set { currentHealth += value; }
    }
}
