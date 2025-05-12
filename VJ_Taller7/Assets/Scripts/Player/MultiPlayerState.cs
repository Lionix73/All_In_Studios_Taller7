using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiPlayerState : NetworkBehaviour
{
    private NetworkVariable <bool> isReady = new NetworkVariable<bool> ();
    public bool IsReady
    {
        get { return isReady.Value; }
        set { if(IsServer) isReady.Value = value; }
    }

    // Variables de red para el puntaje
    private NetworkVariable<int> playerKills = new NetworkVariable<int>();
    private NetworkVariable<int> playerDamage = new NetworkVariable<int>();
    private NetworkVariable<int> playerScore = new NetworkVariable<int>();


    public delegate void PlayerIsReady();
    public event PlayerIsReady OnPlayerReady; //Este evento es para avisar al player manager, luego ese avisa a todos
    public delegate void ScoreUpdated();
    public event ScoreUpdated OnScoreUpdated;

    private HealthMulti health;

    public int Kills => playerKills.Value;
    public int Damage => playerDamage.Value;
    public int Score => playerScore.Value;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        health = GetComponent<HealthMulti>();

        health.OnHealthChanged += UIHealthChangedRpc;
        IsReady = false;

        // Inicializar puntajes
        if (IsServer)
        {
            playerKills.Value = 0;
            playerDamage.Value = 0;
            playerScore.Value = 0;
        }
        if (!IsOwner) return;
        // Suscribirse a cambios en los puntajes
        playerKills.OnValueChanged += (oldValue, newValue) => UpdateScoreUI();
        playerDamage.OnValueChanged += (oldValue, newValue) => UpdateScoreUI();
        playerScore.OnValueChanged += (oldValue, newValue) => UpdateScoreUI();

    }

    [Rpc(SendTo.Server)]
    public void AddKillServerRpc()
    {
        playerKills.Value++;
        playerScore.Value += 100; // 100 puntos por kill, ajusta según necesites
        OnScoreUpdated?.Invoke();
    }

    [Rpc(SendTo.Server)]
    public void AddDamageServerRpc(int damageAmount)
    {
        playerDamage.Value += damageAmount;
        playerScore.Value += damageAmount / 2; // Medio punto por cada punto de daño
        OnScoreUpdated?.Invoke();
    }

    [Rpc(SendTo.Server)]
    public void AddScoreServerRpc(int scoreAmount)
    {
        playerScore.Value += scoreAmount;
        OnScoreUpdated?.Invoke();
    }

    private void UpdateScoreUI()
    {
        if (IsOwner)
        {
           UIManager.Singleton.GetPlayerActualScore(playerScore.Value);
        }
    }
    public void OnReady(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (IsReady) return;

        if(context.started)
        {
            ChangePlayerStateRpc();
        }
    }
    [Rpc(SendTo.Server)]
    public void ChangePlayerStateRpc()
    {
        IsReady = true;
        OnPlayerReady?.Invoke();

    }

    [Rpc(SendTo.Everyone)]
    public void UIHealthChangedRpc(int currentHealth, float maxHealth)
    {
        if (IsOwner)
        {
            UIManager.Singleton.GetPlayerHealth(currentHealth, maxHealth);
        }

    }

}
public enum PlayerState
{
    Wait,
    Playing,
    End
}
