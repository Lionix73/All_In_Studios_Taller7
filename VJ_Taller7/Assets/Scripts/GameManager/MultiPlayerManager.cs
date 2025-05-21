using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MultiPlayerManager : NetworkBehaviour
{
    public static MultiPlayerManager Instance;

    [Tooltip("Lista de jugadores activos en la partida")]
    public Dictionary<ulong, PlayerData> activePlayers = new Dictionary<ulong, PlayerData>();

    [Header("Health Settings")]
    [SerializeField] private int playerStartingHealth = 100;
    [SerializeField] private int healPerRound = 25;
    [SerializeField] private float maxHealthIncreasePerRound = 10f;

    [Header("Respawn Settings")]
    [SerializeField] private float respawnCD = 5f;
    public GameObject activePlayer { get; private set; } //Depronto toca hacer una lista de esto para el multi

    [Tooltip("Este es el player como tal, su codigo y funciones. Dejar vacio en el inspector")]
    private Transform playerPos;
    private HealthMulti playerHealth;
    public GunManager gunManager { get; private set; }

    [Header("Health Scale")]
    private float playerCurrentHealth; //Solo es reflejo de la vida del jugador, no cambiar
    public float PlayerCurrentHealth { get { return playerCurrentHealth; } private set { playerCurrentHealth = value; } }
    private float playerMaxHealth; //Solo es el reflejo de la vida del jugador, no cambiar

    NetworkVariable<int> playersDead = new NetworkVariable<int>(0);
    public int PlayersDead
    { get { return playersDead.Value; }

        set { playersDead.Value = value; }
    }

    NetworkVariable<int> playersReady = new NetworkVariable<int>();
    public int PlayersReady 
    {
        get { return playersDead.Value; }

        set { playersDead.Value = value; }
    }

    public delegate void StartGame();
    public event StartGame OnGameStart;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void SpawnPlayer(GameObject playerPrefab, Transform spawntPoint) {
        // spawn player
        /* if (GameObject.Find("PlayerController") != null) {
             activePlayer = GameObject.Find("PlayerController");
             activePlayer.SetActive(true);//no funciona el desactivados xd
         }
         else {
             activePlayer = Instantiate(playerPrefab, spawntPoint.position, Quaternion.identity);
         }
         if (activePlayer == null) {
             Debug.LogError("Player not found");
             return;
         }*/
        //playerController = activePlayer.GetComponentInChildren<PlayerControllerMulti>(); //No es game object sino el player comom tal
        //gunManager = activePlayer.GetComponentInChildren<GunManager>();

        activePlayer = playerPrefab;
        playerPos = activePlayer.GetComponent<PlayerControllerMulti>().gameObject.transform;
        playerPos.position = spawntPoint.position;

        playerHealth = activePlayer.GetComponent<HealthMulti>();
        //playerHealth.OnHealthChanged += OnPlayerHealthChanged;
        //playerHealth.OnPlayerDeath += OnPlayerDeath;
        //Setear vida inicial
        //playerHealth.SetInitialHealth(playerStartingHealth);

        MultiGameManager.Instance.PlayerSpawn();
    }
    // Registra un nuevo jugador en el manager
    public void RegisterPlayer(ulong clientId, GameObject playerObject)
    {
        if (!activePlayers.ContainsKey(clientId))
        {
            var playerController = playerObject.GetComponentInChildren<PlayerControllerMulti>();
            var playerHealth = playerObject.GetComponentInChildren<HealthMulti>();
            var playerAnimator = playerObject.GetComponentInChildren<Animator>();
            var playerState = playerObject.GetComponentInChildren<MultiPlayerState>();
            var playerGunManager = playerObject.GetComponentInChildren<GunManagerMulti2>();
            var uiManager = GameObject.FindFirstObjectByType<UIManager>();

            var playerData = new PlayerData
            {
                playerObject = playerObject,
                playerController = playerController,
                playerHealth = playerHealth,
                playerState = playerState,
                playerAnimator = playerAnimator,
                playerGunManager = playerGunManager
            };

            activePlayers.Add(clientId, playerData);

            // Configurar eventos
            playerHealth.OnHealthChanged += (current, max) => HandleHealthChanged(clientId, current, max);
            playerHealth.OnPlayerDeath += (player) => HandlePlayerDeath(clientId, player);
            playerState.OnPlayerReady += () => HandlePlayerReady(clientId);

            // Inicializar salud
            playerHealth.SetInitialHealth(playerStartingHealth);
            Debug.Log($"Player {clientId} registered in MultiPlayerManager");
        }
    }
    // Maneja el cambio de salud de un jugador
    private void HandleHealthChanged(ulong clientId, float currentHealth, float maxHealth)
    {
        if (!IsServer) return;

        Debug.Log("Health desde player manager");
        if (activePlayers.TryGetValue(clientId, out PlayerData playerData))
        {
            // Actualizar UI si es el jugador local
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                // UIManager.Singleton?.UpdatePlayerHealth(currentHealth, maxHealth);
            }
        }
    }

    public GunManagerMulti2 GetPlayerGunManager(ulong clientId)
    {
        if (activePlayers.TryGetValue(clientId, out PlayerData playerData))
        {
            Debug.Log("Obtuvimos Gun Manager");

            return playerData.playerGunManager;
        }
        else
        {
            Debug.Log("No Hay Gun Manager");
            return null;
        }
    }
    /*private void OnHealthChanged(float currentHealth, float maxHealth) {
        playerCurrentHealth = currentHealth;
        playerMaxHealth = maxHealth;

        UISet();
    } */
    private void UISet() {
        if (UIManager.Singleton == null) return;
        UIManager.Singleton.GetPlayerHealth(playerCurrentHealth, playerMaxHealth);
    }

    public void HandlePlayerDeath(ulong clientId, GameObject player) {
        if (!IsServer) return;
        //Logica de la muerte del jugador
        if (activePlayers.TryGetValue(clientId, out PlayerData playerData))
        {
            Debug.Log($"Player {clientId} died");

            // Verificar si todos los jugadores están muertos
            CheckAllPlayersDead();

            // Iniciar respawn si es necesario
            StartCoroutine(RespawnPlayer(clientId));
        }
    }

    // Verifica si todos los jugadores están muertos
    private void CheckAllPlayersDead()
    {
        if (!IsServer) { return; }

        PlayersDead = 0;
        foreach (var player in activePlayers.Values)
        {
            if (player.playerHealth.IsDead)
            {
                PlayersDead++;
                Debug.Log($"Players died: {playersDead.Value} players Active: {activePlayers.Count}");
            }
        }
        
        if (PlayersDead == activePlayers.Count)
        {
           MultiGameManager.Instance.GameOver();
        }
    }
    // Corrutina para respawnear jugador
    private IEnumerator RespawnPlayer(ulong clientId)
    {
        yield return new WaitForSeconds(respawnCD);

        if (activePlayers.TryGetValue(clientId, out PlayerData playerData))
        {
            // Aquí deberías tener lógica para obtener el punto de respawn apropiado
            Transform spawnPoint = GetSpawnPointForPlayer(clientId);

            // Reactivar jugador
            playerData.playerObject.SetActive(true);
            playerData.playerObject.transform.position = spawnPoint.position;

            // Resetear salud
            playerData.playerHealth.SetInitialHealth(playerStartingHealth);
            Debug.Log($"Player {clientId} respawned");
            CheckAllPlayersDead();
        }
    }
    private Transform GetSpawnPointForPlayer(ulong clientId)
    {
        // Implementa lógica para obtener spawn point según el jugador
        // Esto puede ser desde un SpawnManager o puntos designados
        return transform; // Ejemplo simplificado
    }

    public void RoundComplete()
    {
        foreach (var player in activePlayers.Values)
        {
            if (!player.playerHealth.IsDead)
            {
                player.playerHealth.TakeHeal(healPerRound);
            }
            player.playerHealth.ScaleHealth(maxHealthIncreasePerRound);

        }
    }
    public void HandlePlayerReady(ulong clientId)
    {
        if (!IsServer) return;
        //Logica de la muerte del jugador
        if (activePlayers.TryGetValue(clientId, out PlayerData playerData))
        {
            Debug.Log($"Player {clientId} is Ready");

            // Verificar si todos los jugadores están muertos
            CheckAllPlayersAreReady();

        }
    }
    public void CheckAllPlayersAreReady()
    {

        foreach (var player in activePlayers.Values)
        {
            if (player.playerState.IsReady)
            {
                PlayersReady++;
            }
            else
            {
                PlayersReady--;
            }
        }

        if (PlayersReady == activePlayers.Count)
        {
            Debug.Log("Start Game");
            OnGameStart?.Invoke();
            MultiGameManager.Instance.PlayGame();
        }
    }

  /*  public void RespawnPlayerOrder(GameObject playerPrefab, Transform spawntPoint){
        StartCoroutine(RespawnPlayer(playerPrefab,spawntPoint));
    }
    private IEnumerator RespawnPlayer(GameObject playerPrefab, Transform spawntPoint){
        yield return new WaitForSeconds(respawnCD);
        activePlayer.SetActive(false);
        //Destroy(activePlayer); //Toca desasignarlo de todas partes... osea, nos embalamos
        activePlayer =null;
        SpawnPlayer(playerPrefab,spawntPoint);
    }*/

    // Estructura para almacenar datos del jugador
    public struct PlayerData
    {
        public ulong playerId;
        public GameObject playerObject;
        public PlayerControllerMulti playerController;
        public HealthMulti playerHealth;
        public MultiPlayerState playerState;
        public Animator playerAnimator;
        public GunManagerMulti2 playerGunManager;
        public UIManager uiManager;
    }
}
