using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Camera Camera { get; set; }

    private void Awake() {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerSpawned += GetPlayer;
        }
    }
    private void Start()
    {
        if(Camera == null){
            Camera = Camera.main;
        }
    }

    private void Update()
    {
        if (Camera == null) return;
        transform.LookAt(Camera.transform, Vector3.up);
    }

    private void GetPlayer(GameObject player)
    {
        Camera = player.GetComponentInChildren<Camera>();
    }
}
