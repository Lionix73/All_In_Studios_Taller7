using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class HealthMulti : NetworkBehaviour, IDamageable
{
    [SerializeField] private TextMeshProUGUI healthDisplay;
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    public int CurrentHealth
    {
        get => currentHealth.Value;
        set { if (IsServer) currentHealth.Value = value; }
    }

    private NetworkVariable<float> maxHealth = new NetworkVariable<float>();
    public float MaxHealth
    {
        get => maxHealth.Value;
        set { if (IsServer) maxHealth.Value = value; }
    }

    public delegate void HealthChanged(float currentHealth, float maxHealth);
    public event HealthChanged OnHealthChanged;

    public delegate void PlayerDeath(GameObject player);
    public event PlayerDeath OnPlayerDeath; //Este evento es para avisar al player manager, luego ese avisa a todos
    private NetworkVariable<bool> isDead = new NetworkVariable<bool>();
    public bool IsDead
    {
        get => isDead.Value;
        set { if (IsServer) isDead.Value = value; }
    }

    [SerializeField] Animator animator;
    private PlayerControllerMulti pController;

    private NetworkVariable<ulong> currentPlayer = new NetworkVariable<ulong>();

    private void Awake()
    {
        pController = GetComponent<PlayerControllerMulti>();
    }

    void Update()
    {
        if (UIManager.Singleton != null)
        {
            UIManager.Singleton.GetPlayerHealth(currentHealth.Value, MaxHealth);
        }
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            healthDisplay = GameObject.Find("HealthDisplay").GetComponent<TextMeshProUGUI>();

        }
        NetworkObject player = GetComponentInParent<NetworkObject>();
        if (IsServer)
        {
            MultiGameManager.Instance.SpawnPlayer(OwnerClientId , gameObject);
        }
    }

    /// <summary>
    /// Se llama desde el player manager para los valores inicales de la vida al aparce el jugador.
    /// </summary>
    /// <param name="startingHealth"></param>
    /// 
    public void SetInitialHealth(float startingHealth){
        MaxHealth = startingHealth;
        CurrentHealth = (int)MaxHealth;
        MovementPlayerStateRpc(true);

        HealthChange(CurrentHealth);
        IsDead = false;
    }
    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        CurrentHealth -= damage;
        TakeDamageRpc(CurrentHealth);

    }
    [Rpc(SendTo.Everyone)]
    public void TakeDamageRpc(int updatedHealth)
    { 
        if(IsServer)
        {
            HealthChange(updatedHealth);
            
            if(updatedHealth <= 0)
            {
                IsDead = true;
                OnPlayerDeath.Invoke(gameObject);
            }
        }

        if (!IsOwner) return;

        if (updatedHealth <= 0)
        {
            MovementPlayerStateRpc(false);
        }
        else
        {
            animator.SetTrigger("Hit");
        }

    }

    [Rpc(SendTo.Everyone)]
    public void MovementPlayerStateRpc(bool state)
    {
        if (!IsOwner) return;

        pController.PlayerCanMove = state;
        pController.PlayerCanJump = state;
        
        if(state)
        {
            animator.SetBool("IsAlive", state);
        }
        else
        {
            animator.SetTrigger("Dead");
        }
    }
    /// <summary>
    /// Heal the player
    /// </summary>
    /// <param name="amount"></param>
    public void TakeHeal(int amount)
    {
        if(IsDead) return;

        CurrentHealth += amount;
        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = (int)MaxHealth; //No sobrepasar el maximo de vida, tener en cuenta para posibles mejoras
        }
        HealthChange(CurrentHealth);
    }

    public void ScaleHealth(float amount){
        MaxHealth += amount;
        if(CurrentHealth > MaxHealth) CurrentHealth = (int)MaxHealth;

        HealthChange(CurrentHealth);
    }
    //Invoke the evento de que cambio la vida, llamar cada vez que ocurre un cambio
    
    private void HealthChange(int updatedHealth)
    {
        HealthChangeRpc(updatedHealth);
        OnHealthChanged?.Invoke(updatedHealth, MaxHealth);
    }
    [Rpc(SendTo.Everyone)]
    public void HealthChangeRpc(int updatedHealth)
    {
        if (!IsOwner) return;
        healthDisplay.text = $"{updatedHealth} / {MaxHealth}";
    }
    public Transform GetTransform()
    {
        return transform;
    }

    public GameObject GetPlayer()
    {
        return gameObject;
    }
}
