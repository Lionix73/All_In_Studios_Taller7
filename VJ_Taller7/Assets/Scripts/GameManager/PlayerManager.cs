using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject activePlayer { get; private set; } //Depronto toca hacer una lista de esto para el multi
    
    [Tooltip("Este es el player como tal, su codigo y funciones. Dejar vacio en el inspector")] 
    public PlayerController playerController;
    private Transform playerPos;
    private Health playerHealth;
    public GunManager gunManager { get; private set; }

    [Header("Health Scale")]
    private float playerCurrentHealth; //Solo es reflejo de la vida del jugador, no cambiar
    private float playerMaxHealth; //Solo es el reflejo de la vida del jugador, no cambiar
    [SerializeField] private float playerStartingHealth; //Tengo que ver para cambiarla desde el spawn
    [SerializeField] private float healPerRound;
    [SerializeField] private float maxHealthIncreasePerRound;

    [Header("Respawn")]
    [SerializeField] private float respawnCD;
    public void SpawnPlayer(GameObject playerPrefab, Transform spawntPoint) {
        // spawn player
        if (GameObject.Find("PlayerController") != null) {
            activePlayer = GameObject.Find("PlayerController");
            activePlayer.SetActive(true);//no funciona el desactivados xd
        }
        else {
            activePlayer = Instantiate(playerPrefab, spawntPoint.position, Quaternion.identity);
        }
        if (activePlayer == null) {
            Debug.LogError("Player not found");
            return;
        }
        playerController = activePlayer.GetComponentInChildren<PlayerController>(); //No es game object sino el player comom tal
        gunManager = activePlayer.GetComponentInChildren<GunManager>();

        playerPos = activePlayer.GetComponentInChildren<PlayerController>().gameObject.transform;
        playerPos.position = spawntPoint.position;

        playerHealth = activePlayer.GetComponentInChildren<Health>();
        playerHealth.OnHealthChanged += OnHealthChanged;
        playerHealth.OnPlayerDeath += PlayerDie;
        //Setear vida inicial
        playerHealth.SetInitialHealth(playerStartingHealth);

        GameManager.Instance.PlayerSpawn();
    }

    private void OnHealthChanged(float currentHealth, float maxHealth) {
        playerCurrentHealth = currentHealth;
        playerMaxHealth = maxHealth;

        UISet();
    }
    private void UISet(){
        if (UIManager.Singleton == null) return;
        UIManager.Singleton.GetPlayerHealth(playerCurrentHealth, playerMaxHealth);
    }

    public void RoundComplete(){
        playerHealth.TakeHeal(healPerRound);
        playerHealth.ScaleHealth(maxHealthIncreasePerRound);

        //Agregar tambien el escalado de las armas desde este manager. Por determinar como
    }

    public void PlayerDie(GameObject player){
        //Logica de la muerte del jugador
        GameManager.Instance.PlayerDied(player);
    }

    public void RespawnPlayerOrder(GameObject playerPrefab, Transform spawntPoint){
        StartCoroutine(RespawnPlayer(playerPrefab,spawntPoint));
    }
    private IEnumerator RespawnPlayer(GameObject playerPrefab, Transform spawntPoint){
        yield return new WaitForSeconds(respawnCD);
        activePlayer.SetActive(false);
        //Destroy(activePlayer); //Toca desasignarlo de todas partes... osea, nos embalamos
        activePlayer =null;
        SpawnPlayer(playerPrefab,spawntPoint);
    }
}
