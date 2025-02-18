using UnityEngine;

public class FloatingText : ObjectLookAtCamera
{
   // [SerializeField] private Camera mainCamera;
    [SerializeField] private float destroyTime = 3f;
    //[SerializeField] private Vector3 offset = new Vector3 (0, 2, 0);
    [SerializeField] private Vector3 randomizeIntensity = new Vector3(0.5f, 0, 0);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, destroyTime);
        LookAtCamera();
        ApplyOffset();
        transform.position += new Vector3(Random.Range(-randomizeIntensity.x, randomizeIntensity.x), Random.Range(-randomizeIntensity.y, randomizeIntensity.y), Random.Range(-randomizeIntensity.z, randomizeIntensity.z));
    }

}
