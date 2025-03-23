using UnityEngine;

[RequireComponent(typeof(PlayerManager))]
[RequireComponent(typeof(RoundManager))]
[RequireComponent(typeof(EnemyWavesManager))]
[RequireComponent(typeof(ScoreManager))]
[DefaultExecutionOrder(-5)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public bool isPaused;
    public bool isGameOver;
    [Tooltip("Si queremos que spawnee un jugador, mas que nada para el editor")]
    public bool spawnPlayerWithManager = true;

    public Transform spawntPoint;
    public GameObject playerPrefab;
    
    //public List<GameObject> activePlayers = new List<GameObject>();

    [Header("Managers")]
    public PlayerManager playerManager;
    public RoundManager roundManager;
    public EnemyWavesManager enemyWavesManager;
    public ScoreManager scoreManager;
    public GunManager gunManager;

    public delegate void OnPlayerSpawn(GameObject player); //Para que todo lo que necesite al player lo encuentre
    public delegate void OnPlayerDeath(GameObject player);
    public event OnPlayerSpawn PlayerSpawned;
    public event OnPlayerDeath PlayerDie;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }

        playerManager = GetComponent<PlayerManager>();
        roundManager = GetComponent<RoundManager>();
        enemyWavesManager = GetComponent<EnemyWavesManager>();
        scoreManager = GetComponent<ScoreManager>();
    }

    private void Start() {
        // Start the game
        if (spawnPlayerWithManager) SpawnPlayer();

        // Crear la logica para el juego en si

    }

    [ContextMenu("SpawnPlayer")]
    public void SpawnPlayer() {
        // spawn player\
        playerManager.SpawnPlayer(playerPrefab, spawntPoint);
        gunManager = playerManager.gunManager;
    }
    public void PlayerDied(GameObject player) {
        PlayerDie?.Invoke(player);
    }
    //Para que todo lo que necesite al player lo encuentre una vez que se haya creado; NOTA: Siempre al final de la función
    public void PlayerSpawn(){
        PlayerSpawned?.Invoke(playerManager.activePlayer); 
    }

    
}
