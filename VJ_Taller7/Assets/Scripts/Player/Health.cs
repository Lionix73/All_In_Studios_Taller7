using TMPro;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private TextMeshProUGUI healthDisplay;
    [SerializeField] private float currentHealth { get; set; }
    [SerializeField] private float maxHealth; 
    public delegate void HealthChanged(float currentHealth, float maxHealth);
    public event HealthChanged OnHealthChanged;

    public delegate void PlayerDeath(GameObject player);
    public event PlayerDeath OnPlayerDeath; //Este evento es para avisar al player manager, luego ese avisa a todos
    void Update()
    {
        if (UIManager.Singleton !=null)
        {
            UIManager.Singleton.GetPlayerHealth(currentHealth, maxHealth);
        } 
    }

    /// <summary>
    /// Se llama desde el player manager para los valores inicales de la vida al aparce el jugador.
    /// </summary>
    /// <param name="startingHealth"></param>
    public void SetInitialHealth(float startingHealth){
        maxHealth = startingHealth;
        currentHealth = maxHealth;

        HealthChange();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        HealthChange();
        if (currentHealth <= 0)
        {
            OnPlayerDeath?.Invoke(gameObject);
        }
    }
    /// <summary>
    /// Heal the player
    /// </summary>
    /// <param name="amount"></param>
    public void TakeHeal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth; //No sobrepasar el maximo de vida, tener en cuenta para posibles mejoras
        }
        HealthChange();
    }

    public void ScaleHealth(float amount){
        maxHealth += amount;
        HealthChange();
    }
    //Invoke the evento de que cambio la vida, llamar cada vez que ocurre un cambio
    private void HealthChange()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    public Transform GetTransform()
    {
        return transform;
    }
}
