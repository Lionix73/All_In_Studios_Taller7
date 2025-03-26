using UnityEngine;

public class ObjectLookAtCamera : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, 0);
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void LookAtCamera()
    {
        transform.LookAt(mainCamera.transform, Vector3.up);
    }
    public void ApplyOffset()
    {
        transform.position += offset;
    }
    void Awake()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.PlayerSpawned += GetPlayer;
        }
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void GetPlayer(GameObject player)
    {
        mainCamera = player.GetComponentInChildren<Camera>();
    }
}
