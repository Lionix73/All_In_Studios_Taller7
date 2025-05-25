using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

namespace MLAPI.Prototyping
{
    /// <summary>
    /// A prototype component for syncing NavMeshAgents
    /// </summary>
    [AddComponentMenu("MLAPI/NetworkNavMeshAgent")]
    [RequireComponent(typeof(NavMeshAgent))]
    public class NetworkNavMeshAgent : NetworkBehaviour
    {
        private NavMeshAgent m_Agent;
        private EnemyMulti enemy;
        /// <summary>
        /// Is proximity enabled
        /// </summary>
        public bool EnableProximity = false;

        /// <summary>
        /// The proximity range
        /// </summary>
        public float ProximityRange = 50f;

        /// <summary>
        /// The delay in seconds between corrections
        /// </summary>
        public float CorrectionDelay = 3f;

        //TODO rephrase.
        /// <summary>
        /// The percentage to lerp on corrections
        /// </summary>
        [Tooltip("Everytime a correction packet is received. This is the percentage (between 0 & 1) that we will move towards the goal.")]
        public float DriftCorrectionPercentage = 0.5f;

        /// <summary>
        /// Should we warp on destination change
        /// </summary>
        public bool WarpOnDestinationChange = false;

        [Header("Network Sync Settings")]
        [SerializeField] private float snapDistanceThreshold = 1.5f; // Distancia para hacer snap
        [SerializeField] private float lerpSpeed = 10f; // Velocidad de interpolaci?n normal

        private void Awake()
        {
            m_Agent = GetComponent<NavMeshAgent>();
            enemy = GetComponent<EnemyMulti>();
        }

        private Vector3 m_LastDestination = Vector3.zero;
        private float m_LastCorrectionTime = 0f;

        private void Update()
        {
            if (!IsServer || enemy.IsDead || !gameObject.activeInHierarchy) return;
            if (m_Agent.destination != m_LastDestination)
            {
                m_LastDestination = m_Agent.destination;
                if (!EnableProximity)
                {
                    OnNavMeshStateUpdateClientRpc(m_Agent.destination, m_Agent.velocity, transform.position);
                }
                else
                {
                    var proximityClients = new List<ulong>();
                    foreach (KeyValuePair<ulong, NetworkClient> client in NetworkManager.Singleton.ConnectedClients)
                    {
                        if (client.Value.PlayerObject == null || Vector3.Distance(client.Value.PlayerObject.transform.position, transform.position) <= ProximityRange)
                        {
                            proximityClients.Add(client.Key);
                        }
                    }

                    OnNavMeshStateUpdateClientRpc(m_Agent.destination, m_Agent.velocity, transform.position);
                }
            }

            if (NetworkManager.Singleton.NetworkTimeSystem.LocalTime - m_LastCorrectionTime >= CorrectionDelay)
            {
                if (!EnableProximity)
                {
                    //OnNavMeshCorrectionUpdateClientRpc(m_Agent.velocity, transform.position);
                }
                else
                {
                    var proximityClients = new List<ulong>();
                    foreach (KeyValuePair<ulong, NetworkClient> client in NetworkManager.Singleton.ConnectedClients)
                    {
                        if (client.Value.PlayerObject == null || Vector3.Distance(client.Value.PlayerObject.transform.position, transform.position) <= ProximityRange)
                        {
                            proximityClients.Add(client.Key);
                        }
                    }

                    //OnNavMeshCorrectionUpdateClientRpc(m_Agent.velocity, transform.position, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = proximityClients.ToArray() } });
                }

                m_LastCorrectionTime = (float)NetworkManager.Singleton.NetworkTimeSystem.LocalTime;
            }
        }

        [Rpc(SendTo.Everyone)]
        private void OnNavMeshStateUpdateClientRpc(Vector3 destination, Vector3 velocity, Vector3 serverPosition/*, ClientRpcParams rpcParams = default*/)
        {
            if (IsServer || enemy.IsDead) return;

            // Calcular distancia entre posici?n actual y la del servidor
            float distanceToServer = Vector3.Distance(transform.position, serverPosition);

            // Snap condicional
            if (distanceToServer > snapDistanceThreshold)
            {
                transform.position = new Vector3(serverPosition.x, transform.position.y, serverPosition.z); // Teletransportar si hay gran discrepancia
            }
            else
            {
                // Interpolaci?n suave para diferencias peque?as
                transform.position = Vector3.Lerp(transform.position, new Vector3(serverPosition.x, transform.position.y, serverPosition.z), DriftCorrectionPercentage);
            }

            // Actualizar agente de navegaci?n
           m_Agent.SetDestination(destination);
        }

        [Rpc(SendTo.Everyone)]
        private void OnNavMeshCorrectionUpdateClientRpc(Vector3 velocity, Vector3 position/*, ClientRpcParams rpcParams = default*/)
        {
            if (IsServer || enemy.IsDead) return;
            m_Agent.Warp(Vector3.Lerp(transform.position, position, DriftCorrectionPercentage));
            //m_Agent.velocity = velocity;
        }
    }
}