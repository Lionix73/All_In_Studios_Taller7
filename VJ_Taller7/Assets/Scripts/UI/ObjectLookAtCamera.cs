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
        if(GameManager.Instance != null)
        {
            GameManager.Instance.PlayerSpawned += GetPlayer;
        }
    }

    private void GetPlayer(GameObject player)
    {
        mainCamera = player.GetComponentInChildren<Camera>();
    }
}
