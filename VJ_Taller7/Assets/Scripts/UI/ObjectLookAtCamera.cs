using UnityEngine;

public class ObjectLookAtCamera : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, 0);
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void LookAtCamera()
    {
        if (mainCamera==null) return;
        transform.LookAt(mainCamera.transform, Vector3.up);
    }
    public void ApplyOffset()
    {
        transform.position += offset;
    }
    private void Awake()
    {
        mainCamera = Camera.main;
    }

}
