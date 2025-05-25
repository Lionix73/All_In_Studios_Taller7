using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class NetworkEnemyMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Enemy enemy;
    [SerializeField] private float updateRate = 0.1f; // 10 updates/seg
    [SerializeField] private float predictionFactor = 0.5f; // Ajusta la agresividad de la predicci�n

    private Vector3 lastServerPosition;
    private Vector3 predictedPosition;
    private float lastUpdateTime;
    private bool isPredicting;

    private void Update()
    {

        // Solo el servidor actualiza la posici�n real
        if (IsServer)
        {
            if (Time.time - lastUpdateTime > updateRate)
            {
                SendPositionUpdate();
                lastUpdateTime = Time.time;
            }
        }
        else if (isPredicting && IsClient)
        {
            // Predicci�n cliente
            ApplyClientPrediction();
        }
    }

    public void SetDestination(Vector3 hit)
    {
        agent.SetDestination(hit);
        SetDestinationRpc(hit);

        // Predicci�n inmediata en cliente
        if (!IsServer)
        {
            StartClientPrediction(hit);
        }
    }

    private void StartClientPrediction(Vector3 destination)
    {
        lastServerPosition = transform.position;
        predictedPosition = destination;
        isPredicting = true;
    }

    private void ApplyClientPrediction()
    {
        // Predice la posici�n usando velocidad actual y factor de predicci�n
        Vector3 predictedMove = agent.velocity * predictionFactor * Time.deltaTime;
        transform.position += new Vector3(predictedMove.x, 0, predictedMove.z); // Solo X/Z
    }

    [Rpc(SendTo.Everyone)]
    private void SetDestinationRpc(Vector3 hit)
    {
        agent.SetDestination(hit);
    }

    private void SendPositionUpdate()
    {
        OnNavMeshStateUpdateClientRpc(agent.destination, agent.velocity, transform.position);
    }

    [Rpc(SendTo.Everyone)]
    private void OnNavMeshStateUpdateClientRpc(Vector3 destination, Vector3 velocity, Vector3 position)
    {
        if (IsServer || enemy.IsDead) return;

        // Corregir predicci�n cuando llega la actualizaci�n del servidor
        if (isPredicting)
        {
            float discrepancy = Vector3.Distance(predictedPosition, position);
            if (discrepancy > agent.radius) // Umbral de correcci�n
            {
                SnapToServerPosition(position);
            }
            isPredicting = false;
        }

        agent.SetDestination(destination);
        agent.velocity = velocity;
    }

    private void SnapToServerPosition(Vector3 position)
    {
        // Correcci�n suave (opcional: puede ser un snap directo)
        transform.position = new Vector3(position.x, transform.position.y, position.z);
    }
}