using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using System.Threading;
using UnityEngine.UI;

public class RoundManager : MonoBehaviour
{
    [Header("Seteo de enemigos")]
    [Tooltip("Lista de los enemigos que existen para seleccionar")]
    public List<BuyableEnemy> enemies = new List<BuyableEnemy>();
    [Header("Manejo de Rondas y oleadas")]
    [SerializeField] private int currentWave = 0; //oleadas
    [SerializeField] private int currentRound =1 ; //Rondas
    private int level = 0; //For the game balance (in case)
    public int CurrentRound {get {return currentRound;}}
    public int CurrentWave { get { return currentWave; } }

    [Tooltip("Para saber si quieren pasar de ronda o esperar")]
    public bool wantToPassRound;
    private bool omnipresentWaveCummingWarning;

    [SerializeField] private int waveSize; //Tamaño de la oleada en cantidad de enemigos 
    private int waveValue;
    private int enemiesKilledOnWave = 0;
    

    [Space]
    [Tooltip("Definir el aumento de valor de la ronda; aumenta la cantidad de enemigos y su dificultad")]
    [SerializeField] private int waveValueScaleMult; //factor de aumento de la cantidad de valor de la wave
    [Tooltip("Tiempo adicional que dura la oleada")]
    [SerializeField] private float waveDurationScaleAdd; //Tiempo que se suma para cada oleada
    [Tooltip("Tiempo maximo al que pueden escalar las oleadas")]
    [SerializeField] private float maximunTimeForWaves = 120;
    
    [Space]
    [Tooltip("Enemigos en cola generados para esta oleada")]
    public List<EnemyScriptableObject> enemiesToSpawn = new List<EnemyScriptableObject>();
    private List<int> enemyIndex;
    private GameObject lastEnemyOfWave; //saber el último enemigo con vida para el drop
    //public Transform[] spawnPoints;
    
    [Space]
    [Tooltip("Tiempo de duración base de las oleadas")]
    public float waveDuration;
    private float waveTimer;
    [Tooltip("Tiempo entre oleadas")]
    [SerializeField] private float inBetweenRoundsWaitTime; //Tiempo entre oleadas
    private float inBetweenRoundsTimer = 30f; //Tiempo entre oleadas
    private bool inBetweenRounds=true;
    private int aliveEnemies;

    [Header("Cosas de Interfaz")]
    [SerializeField] [Tooltip("Mientras encuentre este encuentra todos")]
    private GameObject _RoundUI;
    private TextMeshProUGUI _UiWaveTimer;
    private TextMeshProUGUI _UiBetweenWavesTimer;
    private TextMeshProUGUI _UiWaveCounter;
    private TextMeshProUGUI _UiRoundCounter;
    private TextMeshProUGUI _UiEnemyCounter;
    private bool actualRoundDisplay = false;

    [Header("Managers")]
    private ScoreManager scoreManager;
    private EnemyWaves enemySpawner; //Si se balancea en este manager
    private EnemyWavesManager enemyWavesManager; //Si se balancea en el spawner
    //private ChallengeManager challengeManager; //Pa llamar cuando son los challenges
    
    [Space]
    [SerializeField] private bool _Simulating = false;
    [SerializeField] private bool _BalncingInThis = false;

    //[Header("Eventos")]
    public delegate void WaveStarted();
    public delegate void WaveComplete(bool completeSatisfactory);
    public delegate void RoundComplete();
    public delegate void EnemyKilled();

    public event WaveStarted OnWaveStart;
    public event WaveComplete OnWaveComplete;
    public event RoundComplete OnRoundComplete;
    public event EnemyKilled OnEnemyKilled;

    private RoundsMusicManager _musicRounds;
    private ThisObjectSounds _soundManager;
    private GameObject _nextRoundSkipCircle;

    public void PauseGame(bool state){
        _Simulating = !state;
    }

    private void Awake() {
        _RoundUI = GameObject.Find("RoundsCanva");
        _nextRoundSkipCircle = GameObject.Find("PassRoundCircle");

        _UiWaveTimer = _RoundUI.transform.Find("WaveTimer").GetComponent<TextMeshProUGUI>();
        _UiBetweenWavesTimer = _RoundUI.transform.Find("BetweenWavesTimer").GetComponent<TextMeshProUGUI>();
        _UiWaveCounter = _RoundUI.transform.Find("WaveCounter").GetComponent<TextMeshProUGUI>();
        _UiRoundCounter = _RoundUI.transform.Find("RoundCounter").GetComponent<TextMeshProUGUI>();
        _UiEnemyCounter = _RoundUI.transform.Find("EnemyCounter").GetComponent<TextMeshProUGUI>();

        _musicRounds = FindFirstObjectByType<RoundsMusicManager>();
        _soundManager = transform.parent.GetComponentInChildren<ThisObjectSounds>();
    }
    private void Start()
    {
        scoreManager = GetComponent<ScoreManager>();
        enemySpawner = GetComponent<EnemyWaves>();
        enemyWavesManager = GetComponent<EnemyWavesManager>();
        //challengeManager = GameManager.Instance.challengeManager;

        if (UIManager.Singleton)
        {
            _RoundUI.SetActive(false);
            _Simulating = true;
            UIManager.Singleton.UIChangeImageRound(CurrentRound);
            UIManager.Singleton.ShowPartialPanel("RoundStartUI", 2);

            //challengeManager.ShowChallenges(); //Mostrar los challenges
        }

        SetEnemiesInSpawner();

        wantToPassRound = true; //Las rondas empiezan de una, para el multiplayer controlar esto con lo de darle a la E para empezar;
        omnipresentWaveCummingWarning = true;
        _nextRoundSkipCircle.SetActive(false);
    }
    private void Update() {

        if (currentRound > 3)
        { // ez win
            _Simulating = false;
            _soundManager.QueueSound("CompleteGame");


            GameManager.Instance.WinGame();
            currentRound = 0;
        }
        
        if (aliveEnemies == 0 && !inBetweenRounds && enemiesKilledOnWave>2){
            EndWave(true);
        }

        if (currentWave > 3){
            PassRound();
        }

        if (_Simulating) UpdateTimers();
        
        if (GameManager.Instance.spawnPlayerWithMenu) return;
        UISet();

    }

    /// <summary>
    /// Almacena la lógica del temporizador entre rondas y el de las rondas
    /// <br/>Para limpiar el Update()
    /// </summary>
    private void UpdateTimers(){
        if (inBetweenRounds) {
            if (!wantToPassRound) return; //Para que no pase el tiempo de espera
            if (UIManager.Singleton) UIManager.Singleton.UIBetweenWavesTimer(inBetweenRoundsTimer);
            inBetweenRoundsTimer -= Time.deltaTime;

            if (omnipresentWaveCummingWarning && inBetweenRoundsTimer <= 7) // se supone que el audio dura 7 seg, y el conteo esta calculdo
            {
                //Santi --> Aqui pones que suene el audio del conteo para la ronda, supongo que podes hacer que suene una vez y ya, sino me decis.
                _soundManager.QueueSound("SendingReiforcement");
                omnipresentWaveCummingWarning = false;
            }

            if (inBetweenRoundsTimer <= 0)
            {
                StartWave(); // No pasa a la siguiente ronda si no mata a todos
            }
        }
        else {
            if (UIManager.Singleton) UIManager.Singleton.UIBetweenWaves(waveTimer);
            if (currentRound == 3 && currentWave == 3) return; // Para acabar el juego la idea es que tenga que matar todo lo vivo
            waveTimer -= Time.deltaTime;
            if (waveTimer <= 0){
                EndWave(false);
            }
        }
    }

    private void SetWaveBalance()
    {
        if (_BalncingInThis)
        {
            waveValue = currentWave * waveValueScaleMult * currentRound; //current round en review por balance, seguramente sea un scaleRounsMult
            waveDuration += currentWave * waveDurationScaleAdd;
            if (waveDuration > maximunTimeForWaves) waveDuration = maximunTimeForWaves;

            GenerateEnemies();

            waveTimer = waveDuration;
        }
        else
        {
            waveDuration += currentWave * waveDurationScaleAdd;
            if (waveDuration > maximunTimeForWaves) waveDuration = maximunTimeForWaves;
            //List<WeightedSpawnScriptableObject> temp = GameManager.Instance.GetBalanceWave(currentWave);
            //enemyWavesManager.RecieveWaveLimits(temp);
            //enemyWavesManager.RecieveWaveOrder(level, waveSize);
            StartCoroutine(WaitUIToHideAtStartingWave());
            waveTimer = waveDuration;
        }
    }

    private IEnumerator WaitUIToHideAtStartingWave()
    {
        yield return new WaitForSeconds(3.0f);
        enemyWavesManager.RecieveWaveOrder(level, waveSize);
    }

    private void EndWave(bool how) //Mas comodo y lindo tenerlo todo junto...
    {
        inBetweenRounds = true; //Empezar a descontar para la otra sugiente ronda/oleada
        enemiesKilledOnWave = 0;
        _musicRounds.StopMusic();
        _soundManager.QueueSound(how ? "WinWave" : "FailWave");

        omnipresentWaveCummingWarning = true;

        // if (how)
        // {
        //     //OnWaveComplete?.Invoke(true); //Se completo exitosamente la oleada, solo cuando acaba por matar a todos los enemigos
        //     //_soundManager.PlaySound("WinWave");
        // }
        // else
        // {//End Wave without killing all
        //     //inBetweenRounds = true;
        //     //enemiesKilledOnWave = 0;

        //     //castigar por no completar satisfactoriamente
        //     //aumentar el escalado de los enemigos o repetir
        //     //OnWaveComplete?.Invoke(false);
        //     //_musicRounds.StopMusic();
        //     //_soundManager.PlaySound("FailWave");
        // }

        if (currentWave == 3)
        {
            _nextRoundSkipCircle.SetActive(true);
            UIManager.Singleton.UIInstructionToPass("Hold E to start next ROUND");
            if (currentRound == 3) UIManager.Singleton.UIInstructionToPass("Hold E to END the Game");
        }
        OnWaveComplete?.Invoke(how);
    }
    private void StartWave()
    {
        if (currentRound == 3 && currentWave == 3)
        {
            _soundManager.QueueSound("CompleteGame");
        }
        if (!wantToPassRound) return;
        currentWave++;
        level++;
        SetWaveBalance();

        if (UIManager.Singleton) UIManager.Singleton.UIActualWave(currentWave);

        if (_BalncingInThis) SendWave(enemyIndex); //sin el balance del manager de Alejo (no lo usaremos pero dejemoslo ahi '.')

        inBetweenRounds = false;
        inBetweenRoundsTimer = inBetweenRoundsWaitTime;

        OnWaveStart?.Invoke(); //Comienza la oleada

        _musicRounds.PlayMusic(); // Empezar la musica

        if (currentWave == 3) wantToPassRound = false; //Al iniciar las terceras oleadas queremos que para pasar de ronda ellos decidan.

        if(level == 1)
        {
            _soundManager.QueueSound("IntrudersDetected");
        }
    }
    private void PassRound()
    {
        Debug.Log("Pasar Ronda");
        currentRound++;
        currentWave = 0;
        level++;
            
            if (UIManager.Singleton) 
            { 
                //UIManager.Singleton.actualRoundDisplay = true;
                //UIManager.Singleton.UIChangeRound(currentRound);

                //challengeManager.ShowChallenges(); //Mostrar los challenges
            }

        _musicRounds.StopMusic();
        _soundManager.QueueSound("WinRound");

        OnRoundComplete?.Invoke();
    }

    public void OrderToPassRound()
    {
        if (!wantToPassRound && inBetweenRounds)
        {
            currentWave++;
            inBetweenRoundsTimer = 15; //Para que despues de esperar no tenga que esperar 45 seg; en review
            wantToPassRound = true;

            UIManager.Singleton.UIInstructionToPass("");
            UIManager.Singleton.UIChangeImageRound(currentRound);
            _nextRoundSkipCircle.SetActive(false);
            //Aqui deberia salir el aviso de RONDA 2, el que no tiene oleadas
            //Y en caso de que tengamos el aviso de pasar con la E, tambien aqui...
        }
    }

    public void recieveWaveData(int enemiesToSpawn)
    {
        waveSize = enemiesToSpawn;
    }
    public void enemyHaveSpawn(){
        aliveEnemies += 1;
        if (UIManager.Singleton) UIManager.Singleton.UIEnemiesAlive(aliveEnemies);
    }

    public void GenerateEnemies(){
        List<EnemyScriptableObject> generatedEnmies = new List<EnemyScriptableObject>();
        enemyIndex = new List<int>();
        while(waveValue>0){ //en caso de queres también límite de enemigos poner como condicional ||generatedenemies.count<X
            int randEnemyId = UnityEngine.Random.Range(0,enemies.Count);
            int randEnemyCost = enemies[randEnemyId].spawnCost;

            if (waveValue-randEnemyCost < 0){
                continue;
            }
            generatedEnmies.Add(enemies[randEnemyId].enemyInfo);

            enemyIndex.Add(randEnemyId);
            waveValue -= randEnemyCost;
        }

        enemiesToSpawn.Clear();
        enemiesToSpawn = generatedEnmies;

        //saber cuantos enemigos hay vivos en la oleada
        aliveEnemies = enemiesToSpawn.Count; 
    }

    /// <summary>
    /// Manda la lista de enemigos creados al spawner de enemigos
    /// <br/> En el momento en que se manda es cuando estos hacen spawn
    /// </summary>
    /// <param name="enemiesIndex">Lista con los indices de los enemigos dentro de los disponibles</param>
    private void SendWave(List<int> enemiesIndex){
        enemySpawner.RecibeWaveEnemies(enemiesIndex);
    }

    /// <summary>
    /// Iguala la lista de enemigos disponibles con la del spawner,
    /// <br/> para así que concuerden los indices
    /// </summary>
    private void SetEnemiesInSpawner(){
       enemySpawner.SetEnemiesInSpawner(enemies);
    }

    private void UISet(){
        if (_UiEnemyCounter!=null) _UiEnemyCounter.text = $" Enemigos restantes: \n {aliveEnemies}";
        if (_UiWaveCounter!=null) _UiWaveCounter.text = $"Oleada: {currentWave} /3";
        if (_UiRoundCounter!=null) _UiRoundCounter.text = $"RONDA {currentRound}";

        if (inBetweenRounds){ //depronto hacer un timer general que rote
            if (_UiBetweenWavesTimer!=null) _UiBetweenWavesTimer.text = $"Siguiente ronda en: \n {Mathf.FloorToInt(inBetweenRoundsTimer/60)} : {Mathf.FloorToInt(inBetweenRoundsTimer % 60)}";
            _UiBetweenWavesTimer.gameObject.SetActive(true);
            _UiWaveTimer.gameObject.SetActive(false);
        }
        else{ 
            if (_UiWaveTimer!=null) _UiWaveTimer.text = $"Tiempo restante: \n {Mathf.FloorToInt(waveTimer/60)} : {Mathf.FloorToInt(waveTimer % 60)}";
            _UiBetweenWavesTimer.gameObject.SetActive(false);
            _UiWaveTimer.gameObject.SetActive(true);
        }
        
    }

    #region FUNCIONES SUSCRITAS A EVENTOS
    /// <summary>
    /// Decirle al score manager ocurrio algo que cambio la puntación
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void ChangeScore(Enemy enemy){
        scoreManager.SetScore(enemy.scoreOnKill);
    }

    public void EnemyDied(Enemy enemy)
    {
        if (enemy == null) return;

        OnEnemyKilled?.Invoke();
        _musicRounds.OnEnemyKilled();

        // Use Interlocked for atomic operations
        int currentAlive = Interlocked.Decrement(ref aliveEnemies);
        
        // Thread-safe calculation of enemies killed
        int killed = waveSize - currentAlive;
        Interlocked.Exchange(ref enemiesKilledOnWave, killed);
        
        // Add to score
        scoreManager.AddEnemyKilled(1);
        
        // Update UI with the current value after decrementing
        if(UIManager.Singleton) UIManager.Singleton.UIEnemiesAlive(currentAlive);
    }
#endregion

}

[System.Serializable]
public class BuyableEnemy{
    public EnemyScriptableObject enemyInfo;
    public int spawnCost;

}
