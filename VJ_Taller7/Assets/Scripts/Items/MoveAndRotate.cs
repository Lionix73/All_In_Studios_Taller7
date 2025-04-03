using UnityEngine;

public class MoveAndRotate : MonoBehaviour
{
    [SerializeField] private float amplitude = 1f; // Height of the sine wave
    [SerializeField] private float frequency = 1f; // Speed of the sine wave
    [SerializeField] private float rotationSpeed = 50f; // Rotation speed in degrees per second
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}
