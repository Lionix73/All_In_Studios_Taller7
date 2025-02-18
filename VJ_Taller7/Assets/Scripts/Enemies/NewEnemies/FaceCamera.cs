using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Camera Camera { get; set; }

    private void Start()
    {
        if(Camera == null){
            Camera = Camera.main;
        }
    }

    private void Update()
    {
        transform.LookAt(Camera.transform, Vector3.up);
    }
}
