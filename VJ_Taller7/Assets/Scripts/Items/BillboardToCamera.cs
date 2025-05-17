using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    [Header("Player Camera")]
    [SerializeField] private Transform CameraTransform;

    [Header("Rotation")][Tooltip("Keep rotation only around Y")]
    [SerializeField] private bool onlyRotateAroundY = true;

    private void Start()
    {
        if (CameraTransform == null)
        {
            // Por si no se asign manualmente, toma la cmara principal
            CameraTransform = Camera.main?.transform;
        }
    }

    void LateUpdate()
    {
        if (CameraTransform == null) return;

        Vector3 direction = transform.position - CameraTransform.position;

        if (onlyRotateAroundY)
        {
            direction.y = 0; // Solo rotacin horizontal
        }

        transform.rotation = Quaternion.LookRotation(direction);
    }
}
