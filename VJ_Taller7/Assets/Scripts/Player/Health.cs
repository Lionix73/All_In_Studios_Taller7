using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float currentHealth;
    [SerializeField] private float maxHealth;
    public bool isDead;

    #region Events
    public delegate void HealthChanged(float currentHealth, float maxHealth);
    public event HealthChanged OnHealthChanged;

    public delegate void PlayerDeath(GameObject player);
    public event PlayerDeath OnPlayerDeath; //Este evento es para avisar al player manager, luego ese avisa a todos

    public delegate void PlayerDamage();
    public event PlayerDamage OnPlayerDamage;
    #endregion

    private PlayerController pController;
    private SegundoAliento secondBreath;

    private void Awake()
    {
        pController = GetComponent<PlayerController>();
        secondBreath = GetComponentInChildren<SegundoAliento>();
    }

    #region -----Set up-----
    /// <summary>
    /// Se llama desde el player manager para los valores inicales de la vida al aparce el jugador.
    /// </summary>
    /// <param name="startingHealth"></param>
    public void SetInitialHealth(float startingHealth){
        maxHealth = startingHealth;
        currentHealth = maxHealth;

        HealthChange();
        isDead = false;
    }
    #endregion

    #region -----Damage-----
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        HealthChange();

        if (currentHealth <= 0)
        {
            PlayerDead();
        }
        else
        {
            OnPlayerDamage?.Invoke();
        }
    }
    #endregion

    #region -----Dead-----
    private void PlayerDead()
    {
        if (secondBreath.IsSecondBreathActive) return;

        isDead = true;
        pController.PlayerCanMove = false;
        pController.PlayerCanJump = false;
        OnPlayerDeath?.Invoke(gameObject);
    }
    #endregion

    #region -----Healing-----
    /// <summary>
    /// Heal the player
    /// </summary>
    /// <param name="amount"></param>
    public void TakeHeal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth; //No sobrepasar el maximo de vida, tener en cuenta para posibles mejoras
        }
        HealthChange();
    }
    #endregion

    #region -----Progresion-----
    public void ScaleHealth(float amount)
    {
        maxHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        HealthChange();
    }
    #endregion

    //Invocar el evento del cambio de la vida, llamar cada vez que ocurre un cambio
    private void HealthChange() => OnHealthChanged?.Invoke(currentHealth, maxHealth);

    #region -----Getters-----
    public Transform GetTransform()
    {
        return transform;
    }

    public float GetMaxHeath
    {
        get { return maxHealth; }
    }

    public float GetCurrentHeath
    {
        get { return currentHealth; }
    }
    #endregion
}
