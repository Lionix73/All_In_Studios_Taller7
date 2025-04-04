using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public bool spawnPlayerWithMenu = false;

    public Transform spawntPoint;
    public GameObject playerPrefab;

    //public List<GameObject> activePlayers = new List<GameObject>();

    [Header("Managers")]
    public UIManager UIManager;
    public PlayerManager playerManager;
    public RoundManager roundManager;
    public EnemyWavesManager enemyWavesManager;
    public ScoreManager scoreManager;
    public GunManager gunManager;

    public delegate void OnPlayerSpawn(GameObject player); //Para que todo lo que necesite al player lo encuentre
    public delegate void OnPlayerDeath(GameObject player);
    public event OnPlayerSpawn PlayerSpawned;
    public event OnPlayerDeath PlayerDie;

    //[Header("Balance")]
    [SerializeField] public List<WaveRestriction> availableEnemiesForWave = new List<WaveRestriction>();
    //[SerializeField]private List<WaveDificulty> wavesBalance = new List<WaveDificulty>();

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }

        ObjectPool.ClearPools();
        playerManager = GetComponent<PlayerManager>();
        roundManager = GetComponent<RoundManager>();
        enemyWavesManager = GetComponent<EnemyWavesManager>();
        scoreManager = GetComponent<ScoreManager>();
    }

    private void Start() {
        // Start the game
        if (UIManager.Singleton) spawnPlayerWithMenu = true;
        SpawnPlayer();

        // Crear la logica para el juego en si

    }

    [ContextMenu("SpawnPlayer")]
    public void SpawnPlayer() {
        // spawn player\
        if (spawnPlayerWithMenu){
            playerManager.SpawnPlayer(SelectedPlayer(), spawntPoint);
            gunManager = playerManager.gunManager;
        }
        else{
            playerManager.SpawnPlayer(playerPrefab, spawntPoint);
            gunManager = playerManager.gunManager;
        }
        isGameOver = false;
    }
    public void PlayerDied(GameObject player) {
        PlayerDie?.Invoke(player);
        isGameOver = true; //Primero saber si el otro jugador esta vivo y depues si confirmar el game over
        if(UIManager.Singleton != null) UIManager.Singleton.DiedUI(6);

        if (spawnPlayerWithMenu) return;

        playerManager.RespawnPlayerOrder(playerPrefab,spawntPoint);
        
    }
    //Para que todo lo que necesite al player lo encuentre una vez que se haya creado; NOTA: Siempre al final de la función
    public void PlayerSpawn(){
        PlayerSpawned?.Invoke(playerManager.activePlayer); 
    }
    
    public GameObject SelectedPlayer()
    {
        int selectedIndex = CharacterManager.Instance.selectedIndexCharacter;
        return CharacterManager.Instance.characters[selectedIndex].playableCharacter;
    }

    public void RestartScene(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PauseGame(){
        isPaused = !isPaused;
        if (UIManager.Singleton) roundManager.PauseGame(isPaused);
    }

    /*
    public List<WeightedSpawnScriptableObject> GetBalanceWave(int wave){
        if (wavesBalance[wave-1].enemiesForTheWave == null) return null;
        return wavesBalance[wave-1].enemiesForTheWave;
    }
    */
}

/*
[System.Serializable]
public class WaveDificulty{
    public int waveToSet;
    public List<WeightedSpawnScriptableObject> enemiesForTheWave;
}
*/
[System.Serializable]
public class WaveRestriction{
    public int waveToSet;
    public int[] availableEnemies;
}
