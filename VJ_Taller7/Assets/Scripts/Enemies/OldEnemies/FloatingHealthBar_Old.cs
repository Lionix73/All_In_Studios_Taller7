using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar_Old : MonoBehaviour
{
    [SerializeField] private Slider healthBarSlider;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;

    void Awake(){
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
    }

    void Start()
    {
    
    }

    void Update()
    {
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
        transform.position = target.position + offset;
    }

    public void UpdateHealthBar(float currentValue, float maxValue)
    {
        healthBarSlider.value = currentValue / maxValue;
    }
}
