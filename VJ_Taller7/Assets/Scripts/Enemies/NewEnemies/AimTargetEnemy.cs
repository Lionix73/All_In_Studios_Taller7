using UnityEngine;

public class AimTargetEnemy : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform enemyTransform; // Parent enemy transform
    [SerializeField] private float maxDistanceFromEnemy = 5f; // Maximum distance from the enemy
    [SerializeField] private float followSpeed = 5f; // How fast the aim target moves
    [SerializeField] private Vector3 defaultLocalPosition = Vector3.forward; // Default position relative to enemy
    [SerializeField] private float returnSpeed = 3f; // Speed to return to default position
    [SerializeField] private float playerHeightOffset = 1.5f; // Offset from player's pivot point


    private Transform playerTransform;
    private bool isFollowingPlayer = false;
    private Vector3 targetPosition;

    private void Awake()
    {
        if (enemyTransform == null)
        {
            enemyTransform = transform.parent;
        }
    }

    private void Start()
    {
        // Initialize at default position
        transform.localPosition = defaultLocalPosition;
        targetPosition = transform.position;
    }

    private void Update()
    {
        if (isFollowingPlayer && playerTransform != null)
        {
            // Calculate the direction to the player
            Vector3 playerTargetPosition = playerTransform.position + Vector3.up * playerHeightOffset;
            Vector3 directionToPlayer = playerTargetPosition - enemyTransform.position;
            
            // Limit distance from enemy
            if (directionToPlayer.magnitude > maxDistanceFromEnemy)
            {
                directionToPlayer = directionToPlayer.normalized * maxDistanceFromEnemy;
            }
            
            // Set target position to be in player's direction but limited by max distance
            targetPosition = enemyTransform.position + directionToPlayer;
        }
        else
        {
            // Return to default position relative to enemy
            targetPosition = enemyTransform.position + enemyTransform.TransformDirection(defaultLocalPosition);
        }

        // Smoothly move to target position
        float currentSpeed = isFollowingPlayer ? followSpeed : returnSpeed;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * currentSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            isFollowingPlayer = true;
            playerTransform = player.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the player exited the trigger
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && playerTransform == player.transform)
        {
            isFollowingPlayer = false;
            playerTransform = null;
        }
    }

    // Draw gizmos to visualize the max distance
    private void OnDrawGizmosSelected()
    {
        if (enemyTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(enemyTransform.position, maxDistanceFromEnemy);
            
            // Draw default position
            Gizmos.color = Color.blue;
            Vector3 defaultPos = Application.isPlaying ? 
                enemyTransform.position + enemyTransform.TransformDirection(defaultLocalPosition) : 
                transform.parent.position + transform.parent.TransformDirection(defaultLocalPosition);
            Gizmos.DrawSphere(defaultPos, 0.2f);

            if (playerTransform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(playerTransform.position + Vector3.up * playerHeightOffset, 0.2f);
            }
        }
    }
}