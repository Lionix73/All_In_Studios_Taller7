using UnityEngine;

public class ObjectLookAtCamera : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, 0);
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void LookAtCamera()
    {
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
    }
    public void ApplyOffset()
    {
        transform.position += offset;
    }
    void Awake()
    {
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
    }
}
