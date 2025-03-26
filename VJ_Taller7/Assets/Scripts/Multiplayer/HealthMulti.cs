using TMPro;
using Unity.Netcode;
using UnityEngine;

public class HealthMulti : NetworkBehaviour, IDamageable
{
    [SerializeField] private TextMeshProUGUI healthDisplay;
    private NetworkVariable <float> currentHealth = new NetworkVariable<float>();
    private NetworkVariable <float> maxHealth = new NetworkVariable<float>(); 
    public delegate void HealthChanged(float currentHealth, float maxHealth);
    public event HealthChanged OnHealthChanged;

    public delegate void PlayerDeath(GameObject player);
    public event PlayerDeath OnPlayerDeath; //Este evento es para avisar al player manager, luego ese avisa a todos
    void Update()
    {
        if (UIManager.Singleton !=null)
        {
            UIManager.Singleton.GetPlayerHealth(currentHealth.Value, maxHealth.Value);
        } 
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkObject player = GetComponentInParent<NetworkObject>();
        if (IsServer)
        {
            maxHealth.Value = 100;
            currentHealth.Value = 40;
        }
        if (player.IsLocalPlayer)
        {
            healthDisplay = GameObject.Find("HealthDisplay").GetComponent<TextMeshProUGUI>();
            HealthChange();
        }
    }

    /// <summary>
    /// Se llama desde el player manager para los valores inicales de la vida al aparce el jugador.
    /// </summary>
    /// <param name="startingHealth"></param>
    public void SetInitialHealth(float startingHealth){
        maxHealth.Value = startingHealth;
        currentHealth.Value = maxHealth.Value;

        HealthChange();
    }

    public void TakeDamage(int damage)
    {
        currentHealth.Value -= damage;
        HealthChange();
        if (currentHealth.Value <= 0)
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
        currentHealth.Value += amount;
        if (currentHealth.Value > maxHealth.Value)
        {
            currentHealth.Value = maxHealth.Value; //No sobrepasar el maximo de vida, tener en cuenta para posibles mejoras
        }
    }

    public void ScaleHealth(float amount){
        maxHealth.Value += amount;
        HealthChange();
    }
    //Invoke the evento de que cambio la vida, llamar cada vez que ocurre un cambio
    
    private void HealthChange()
    {
        healthDisplay.text = $"{currentHealth.Value} / {maxHealth.Value}";
        OnHealthChanged?.Invoke(currentHealth.Value, maxHealth.Value);
    }
    [Rpc(SendTo.Everyone)]
    public void HealthChangeRpc()
    {
        if (!IsOwner) return;
        healthDisplay.text = $"{currentHealth.Value} / {maxHealth.Value}";
        OnHealthChanged?.Invoke(currentHealth.Value, maxHealth.Value);
    }
    public Transform GetTransform()
    {
        return transform;
    }
}
