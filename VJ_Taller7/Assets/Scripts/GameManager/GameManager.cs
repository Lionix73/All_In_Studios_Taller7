using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
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
    public bool allowPlayerRespawn;
    [Tooltip("Si queremos que spawnee un jugador, mas que nada para el editor")]
    public bool spawnPlayerWithMenu = false;

    public Transform spawntPoint;
    public GameObject playerPrefab;

    [SerializeField] private GameObject GunShop;

    //public List<GameObject> activePlayers = new List<GameObject>();

    [Header("Managers")]
    public UIManager UIManager;
    public PlayerManager playerManager;
    public RoundManager roundManager;
    public EnemyWavesManager enemyWavesManager;
    public ScoreManager scoreManager;
    public GunManager gunManager;
    public ChallengeManager challengeManager;

    [Tooltip("Se escala este dmg al dmg base del jugador cuando completa una ronda satisfactoriamente")]
    [Header("Escalado")]
    public int amountOfDamagePlayerScalePerWave;

    [Header("Events")] private string paPonerElHeaderXD;

    public delegate void OnPlayerSpawn(GameObject player); //Para que todo lo que necesite al player lo encuentre
    public delegate void OnPlayerDeath(GameObject player);
    public delegate void OnScoreChange(float actualScore);
    public event OnPlayerSpawn PlayerSpawned;
    public event OnPlayerDeath PlayerDie;
    public event OnScoreChange ScoreChanged;

    //[Header("Balance")]
    [SerializeField] public List<WaveRestriction> availableEnemiesForWave = new List<WaveRestriction>();
    //[SerializeField]private List<WaveDificulty> wavesBalance = new List<WaveDificulty>();

    private GameObject cameraAnim;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
        cameraAnim = GameObject.FindGameObjectWithTag("AnimationCamera");
        cameraAnim.SetActive(false);
        ObjectPool.ClearPools();
        playerManager = GetComponent<PlayerManager>();
        roundManager = GetComponent<RoundManager>();
        enemyWavesManager = GetComponent<EnemyWavesManager>();
        scoreManager = GetComponent<ScoreManager>();
        challengeManager = FindFirstObjectByType<ChallengeManager>();
    }

    private void Start() {
        // Start the game
        if (UIManager.Singleton) spawnPlayerWithMenu = true;
        SpawnPlayer();

        // Crear la logica para el juego en si
        roundManager.OnWaveStart += WaveStarted;
        roundManager.OnWaveComplete += WaveFinished;
        roundManager.OnRoundComplete += RoundFinished;

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
            
        }
        isGameOver = false;
    }
    public void PlayerDied(GameObject player) {
        //SetScoreLeaderboard();

        PlayerDie?.Invoke(player);
        isGameOver = true; //Primero saber si el otro jugador esta vivo y depues si confirmar el game over
        cameraAnim.SetActive(true);
        FinalScoreManager.Singleton.UpdateDiedLeaderboard(UIManager.Singleton.GetPlayerName(),(int)scoreManager.GetTotalScore(), (int)scoreManager.GetKilledEnemies());

        if (UIManager.Singleton != null) UIManager.Singleton.FinalUI(false);



        if (spawnPlayerWithMenu) return;

        if (allowPlayerRespawn) playerManager.RespawnPlayerOrder(playerPrefab,spawntPoint);
        
    }
    //Para que todo lo que necesite al player lo encuentre una vez que se haya creado; NOTA: Siempre al final de la función
    public void PlayerSpawn(){
        PlayerSpawned?.Invoke(playerManager.activePlayer); 
        //gunManager = playerManager.gunManager;
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

    public void WinGame(){
        //Evento de victoria si es necesario
        cameraAnim.SetActive(true);

        //SetScoreLeaderboard();
        if (UIManager.Singleton) UIManager.Singleton.FinalUI(true);
        FinalScoreManager.Singleton.UpdateDiedLeaderboard(UIManager.Singleton.GetPlayerName(), (int)scoreManager.GetTotalScore(), (int)scoreManager.GetKilledEnemies());

    }

    public void WaveStarted(){
        GunShop?.GetComponent<ShopLimit>().WaveStarted();
        UIManager.Singleton.UIChangeImageWave(roundManager.CurrentWave);
    }
    
    private void WaveFinished(bool killedAll){
        GunShop?.GetComponent<ShopLimit>().WaveFinished(killedAll);

        if (!killedAll) return;
        UIManager.Singleton.ShowPartialPanel("WaveCompleteUI", 3);
        gunManager.ScaleDamage(amountOfDamagePlayerScalePerWave);
        //El escalado de las armas tengo que mirar si hacerlo que cada arma tenga su esacalado, o un entero pa todas
        //La segunda opcion me gusta mas porque es mas facil de hacer... xd
    }

    private void RoundFinished()
    {
        UIManager.Singleton.UIChangeImageRound(roundManager.CurrentRound);
        playerManager.RoundComplete();
        amountOfDamagePlayerScalePerWave += amountOfDamagePlayerScalePerWave;
    }

    public void ScoreChange(float actualScore){
        ScoreChanged?.Invoke(actualScore);
    }


    /*
    public List<WeightedSpawnScriptableObject> GetBalanceWave(int wave){
        if (wavesBalance[wave-1].enemiesForTheWave == null) return null;
        return wavesBalance[wave-1].enemiesForTheWave;
    }
    */
    public async void SetScoreLeaderboard()
    {
        string leaderboardID = LeaderboardsManager.Singleton.GetLeaderboardID();
        double score = scoreManager.GetScore();

        try
        {
            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardID, score);
        }
        catch (LeaderboardsException e)
        {
            Debug.LogException(e);
        }
    }
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
