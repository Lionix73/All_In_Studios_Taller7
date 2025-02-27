using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

[RequireComponent(typeof(ScoreManager))]
public class RoundManager : MonoBehaviour
{
    [Header("Seteo de enemigos")]
    [Tooltip("Lista de los enemigos que existen para seleccionar")]
    public List<BuyableEnemy> enemies = new List<BuyableEnemy>();
    [Header("Manejo de Rondas y oleadas")]
    [SerializeField] private int currentWave; //oleadas
    [SerializeField] private int currentRound; //Rondas
    private int waveValue;

    [Space]
    [Tooltip("Definir el aumento de valor de la ronda; aumenta la cantidad de enemigos y su dificultad")]
    [SerializeField] private int waveValueScaleMult; //factor de aumento de la cantidad de valor de la wave
    [Tooltip("Tiempo adicional que dura la oleada")]
    [SerializeField] private float waveDurationScaleAdd; //Tiempo que se suma para cada oleada
    
    [Space]
    [Tooltip("Enemigos en cola generados para esta oleada")]
    public List<GameObject> enemiesToSpawn = new List<GameObject>();
    private GameObject lastEnemyOfWave; //saber el último enemigo con vida para el drop
    //public Transform[] spawnPoints;
    private int spawnIndex;
    
    [Space]
    public float waveDuration;
    private float waveTimer;
    private float spawnInterval; //Time between each enemy
    [Tooltip("Tiempo entre oleadas")]
    [SerializeField] private float spawnTimer; //Tiempo entre oleadas
    [SerializeField] private float inBetweenRoundsTimer; //not using now
    private int aliveEnemies;

    [Header("Cosas de Interfaz")]
    [SerializeField] private TextMeshProUGUI _UiWaveTimer;
    [SerializeField] private TextMeshProUGUI _UiWaveCounter;
    [SerializeField] private TextMeshProUGUI _UiRoundCounter;
    [SerializeField] private TextMeshProUGUI _UiEnemyCounter;

    private ScoreManager scoreManager;


    private void Start() {
        scoreManager = GetComponent<ScoreManager>();
        SetWaveBalance();
    }
    private void Update() {
        if (aliveEnemies == 1){
            
        }
    }

    private void SetWaveBalance(){
        waveValue = currentWave * waveValueScaleMult * currentRound ; //current round en review por balance, seguramente sea un scaleRounsMult
        waveDuration += currentWave * waveDurationScaleAdd;

        GenerateEnemies();

        spawnInterval = waveDuration/enemiesToSpawn.Count;
        waveTimer = waveDuration;
    }

    public void GenerateEnemies(){
        List<GameObject> generatedEnmies = new List<GameObject>();

        while(waveValue>0){ //en caso de queres también límite de enemigos poner como condicional ||generatedenemies.count<X
            int randEnemyId = UnityEngine.Random.Range(0,enemies.Count);
            int randEnemyCost = enemies[randEnemyId].spawnCost;

            if (waveValue-randEnemyCost < 0){
                continue;
            }
            generatedEnmies.Add(enemies[randEnemyId].enemyPrefab);
            waveValue -= randEnemyCost;
        }

        enemiesToSpawn.Clear();
        enemiesToSpawn = generatedEnmies;

        foreach(GameObject enemy in enemiesToSpawn){
            enemy.TryGetComponent<Enemy>(out Enemy enemieDead);
            enemieDead.OnEnemyDead += ChangeScore;
        }

        //saber cuantos enemigos hay vivos en la oleada
        aliveEnemies = enemiesToSpawn.Count; 

        //SendWave();
    }

    private void SendWave(){
        GetComponent<EnemyWaves>().WaveSpawn(enemiesToSpawn,spawnInterval);
    }

    private void ChangeScore(object sender, Enemy.OnEnemyDeadEventArgs e){
        scoreManager.SetScore(e.score);

        aliveEnemies -=1; //posiblemente aguanta moverlo a su propia función y suscribirla también al evento
    }

/// <summary>
/// Esta función solo la uso desde el editor, en vista de que el spawn todavía no esta integrado
/// Busca todos los enemigos de la escena y los asigna como si fueran los generados
/// </summary>
    [ContextMenu("SearchEnemiesOnScene")]
    private void SearchEnemiesOnScene(){
        Enemy[] enemiesOnScene = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        enemiesToSpawn.Clear();
        for (int i=0; i < enemiesOnScene.Length-1; i++){
            enemiesOnScene[i].OnEnemyDead += ChangeScore;
            enemiesToSpawn.Add(enemiesOnScene[i].gameObject);
        }
    }

}

[System.Serializable]
public class BuyableEnemy{
    public GameObject enemyPrefab;
    public int spawnCost;

}
