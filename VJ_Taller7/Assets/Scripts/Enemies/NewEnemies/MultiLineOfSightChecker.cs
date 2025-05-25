using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class MultiLineOfSightChecker : NetworkBehaviour
{
    [SerializeField] private float fov = 90f;
    [SerializeField] private MultiEnemyMovement enemyMov;
    public float Fov{ get => fov; set => fov = value; }

    [SerializeField] private LayerMask lineOfSightMask;
    public LayerMask LineOfSightMask{ get => lineOfSightMask; set => lineOfSightMask = value; }

    [SerializeField] private const string playerTag = "Player";

    private SphereCollider sphereCollider;
    public SphereCollider SphereCollider{ get => sphereCollider; set => sphereCollider = value; }

    public delegate void GainSightEvent(HealthMulti player);
    public GainSightEvent OnGainSight;

    public delegate void LoseSightEvent(HealthMulti player);
    public LoseSightEvent OnLoseSight;

    private Coroutine CheckForlineOfSightCoroutine;

    // Lista para mantener track de los jugadores dentro del trigger
    private List<HealthMulti> playersInTrigger = new List<HealthMulti>();
    private HealthMulti currentClosestPlayer;
    private List<HealthMulti> subscribedPlayers = new List<HealthMulti>();

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(!IsServer) return;
        // Suscribirse a todos los jugadores existentes
        HealthMulti[] allPlayers = FindObjectsOfType<HealthMulti>();
        foreach (var player in allPlayers)
        {
            SubscribeToPlayer(player);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.gameObject.tag == playerTag)
        {
            if (other.TryGetComponent<HealthMulti>(out var player))
            {
                // Solo añadir si no está muerto
                if (!player.IsDead)
                {
                    playersInTrigger.Add(player);
                    UpdateClosestPlayer();
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) return;

        // Actualizamos constantemente el jugador más cercano
        if (other.gameObject.tag == playerTag)
        {
            UpdateClosestPlayer();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.gameObject.tag == playerTag)
        {
            if (other.TryGetComponent<HealthMulti>(out var player))
            {
                playersInTrigger.Remove(player);

                // Si el jugador que salió era el más cercano
                if (currentClosestPlayer == player)
                {
                    OnLoseSight?.Invoke(player);
                    if (CheckForlineOfSightCoroutine != null)
                    {
                        StopCoroutine(CheckForlineOfSightCoroutine);
                    }
                    currentClosestPlayer = null;
                    // Buscar un nuevo jugador cercano
                    UpdateClosestPlayer();
                }
            }
        }
    }
    private void UpdateClosestPlayer()
    {
        // Primero limpiamos la lista de jugadores eliminando los nulos o muertos
        playersInTrigger.RemoveAll(player => player == null || player.IsDead);

        if (playersInTrigger.Count == 0)
        {
            if (currentClosestPlayer != null)
            {
                OnLoseSight?.Invoke(currentClosestPlayer);
                currentClosestPlayer = null;
            }
            return;
        }

        // Encontrar el jugador vivo más cercano
        HealthMulti closestPlayer = null;
        float closestDistance = float.MaxValue;

        foreach (var player in playersInTrigger)
        {

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        // Si no encontramos jugadores vivos
        if (closestPlayer == null)
        {
            if (currentClosestPlayer != null)
            {
                OnLoseSight?.Invoke(currentClosestPlayer);
                currentClosestPlayer = null;
            }
            return;
        }

        // Si el jugador más cercano cambió
        if (closestPlayer != currentClosestPlayer)
        {
            // Notificar que perdimos de vista al anterior
            if (currentClosestPlayer != null)
            {
                OnLoseSight?.Invoke(currentClosestPlayer);
                if (CheckForlineOfSightCoroutine != null)
                {
                    StopCoroutine(CheckForlineOfSightCoroutine);
                }
            }

            currentClosestPlayer = closestPlayer;

            // Comenzar a verificar la línea de visión con el nuevo jugador más cercano
            if (!CheckLineOfSight(currentClosestPlayer))
            {
                CheckForlineOfSightCoroutine = StartCoroutine(CheckForLineOfSight(currentClosestPlayer));
            }
        }
    }
    private bool CheckLineOfSight(HealthMulti player){

        if (player == null) return false;

        Vector3 direction = (player.transform.position - transform.position).normalized;

        Debug.DrawRay(transform.position, direction * sphereCollider.radius, Color.red);

        if(Vector3.Dot(transform.forward, direction) >= Mathf.Cos(fov)){
            RaycastHit hit;

            if(Physics.Raycast(transform.position, direction, out hit, sphereCollider.radius, lineOfSightMask)){
                if(hit.transform.GetComponent<PlayerControllerMulti>() != null){
                    OnGainSight?.Invoke(player);
                    return true;
                }
            }
        }

        return false;
    }

    private IEnumerator CheckForLineOfSight(HealthMulti player){
        WaitForSeconds wait = new WaitForSeconds(0.1f);

        while(!CheckLineOfSight(player)){
            yield return wait;
        }
    }

    private void SubscribeToPlayer(HealthMulti player)
    {
        Debug.Log("Suscribir evento de muerte");
        if (player == null || subscribedPlayers.Contains(player)) return;

        player.OnPlayerDeath += HandlePlayerDeath;
        subscribedPlayers.Add(player);
    }

    private void UnsubscribeFromAllPlayers()
    {
        foreach (var player in subscribedPlayers)
        {
            if (player != null)
            {
                player.OnPlayerDeath -= HandlePlayerDeath;
            }
        }
        subscribedPlayers.Clear();
    }


    private void HandlePlayerDeath(GameObject deadPlayer)
    {
        if (!IsServer) return;
        if (!gameObject.activeInHierarchy) return;
        EnemyMulti enemy = transform.root.GetComponentInChildren<EnemyMulti>();
        // Si el jugador muerto era nuestro objetivo actual
        if (enemy.Movement.Player.gameObject == deadPlayer)
        {
            FindNewClosestPlayer();
        }

    }

    private void FindNewClosestPlayer()
    {
        EnemyMulti enemy = transform.root.GetComponentInChildren<EnemyMulti>();
        Debug.Log("Buscando jugadores vivos...");
        HealthMulti closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (var player in subscribedPlayers)
        {
            if (player == null || player.IsDead) continue;

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }
        currentClosestPlayer = closestPlayer;
        enemy.GetPlayer(closestPlayer);
        // Aquí actualizas la lógica de persecución
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        UnsubscribeFromAllPlayers();
    }
}
