using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
//[RequireComponent(typeof(PlayerManager))]
[RequireComponent(typeof(MultiRoundManager))]
[RequireComponent(typeof(MultiEnemyWavesManager))]
//[RequireComponent(typeof(ScoreManager))]
[DefaultExecutionOrder(-5)]
public class MultiGameManager : NetworkBehaviour
{
    public static MultiGameManager Instance;

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
    public MultiPlayerManager playerManager;
    public MultiRoundManager roundManager;
    public MultiEnemyWavesManager enemyWavesManager;
    public ScoreManager scoreManager;
    public GunManager gunManager;
    //public ChallengeManager challengeManager;

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
        ObjectPoolMulti.ClearPools();
        playerManager = GetComponent<MultiPlayerManager>();
        roundManager = GetComponent<MultiRoundManager>();
        enemyWavesManager = GetComponent<MultiEnemyWavesManager>();
        scoreManager = GetComponent<ScoreManager>();
        //challengeManager = FindFirstObjectByType<ChallengeManager>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // Start the game
        if (UIManager.Singleton) spawnPlayerWithMenu = true;

        //SpawnPlayer();

        // Crear la logica para el juego en si

    }

    [ContextMenu("SpawnPlayer")]
    public void SpawnPlayer(ulong clientId ,GameObject playerGO) {
        playerPrefab = playerGO;
        // spawn player\
        if (spawnPlayerWithMenu){
            playerManager.RegisterPlayer(clientId, playerPrefab);
            isGameOver = false;
        }
        /*else{
            playerManager.SpawnPlayer(playerPrefab, spawntPoint);
            gunManager = playerManager.gunManager;
        }*/
        playerManager.RegisterPlayer(clientId, playerPrefab);
        isGameOver = false;
    }
    public void PlayerDied(GameObject player) {
        PlayerDie?.Invoke(player);
        if (isGameOver)
        {
            LostGameUIRpc();
        }

        if (spawnPlayerWithMenu) return;

        //playerManager.RespawnPlayerOrder(playerPrefab,spawntPoint);
        
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

    public void PlayGame()
    {
        roundManager._Simulating = true;
        StartRoundUIRpc(roundManager.CurrentRound);
    }
    public void GameOver()
    {
        Debug.Log("GameOver");
        LostGameUIRpc();
        isGameOver = true; 
    }
    [Rpc(SendTo.Everyone)]
    public void StartRoundUIRpc(int currentRound) {
        UIManager.Singleton.UIChangeRound(currentRound);
    }

    [Rpc(SendTo.Everyone)]
    public void LostGameUIRpc()
    {
        if (UIManager.Singleton != null) UIManager.Singleton.DiedUI(6);
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
