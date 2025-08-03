using UnityEngine;
using UnityEngine.UI;

public class LoadSavedVolume : MonoBehaviour
{
    [SerializeField] private VolumeController volumeController;
    private Slider slider;

    private void Awake()
    {
        slider = GetComponentInChildren<Slider>();
    }

    void Start()
    {
        LoadValue();
    }

    private void LoadValue()
    {
        float savedValue = volumeController.GetVolume(gameObject.name);

        slider.value = savedValue;
    }
}
