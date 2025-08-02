using UnityEngine;

public class MoveDoor : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Transform closedPosition;
    [SerializeField] private Transform openPosition;
    private Vector3 targetPosition;
    private bool isOpen = false;

    private void Start()
    {
        // Initialize door at closed position
        if (closedPosition != null)
        {
            transform.position = closedPosition.position;
            targetPosition = closedPosition.position;
        }
    }

    private void Update()
    {
        // Smoothly move door towards target position
        if (targetPosition != null && transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }
    
    public void ToggleDoor()
    {
        if (isOpen)
        {
            if (closedPosition != null)
                targetPosition = closedPosition.position;
        }
        else
        {
            if (openPosition != null)
                targetPosition = openPosition.position;
        }
        isOpen = !isOpen;
    }
}
