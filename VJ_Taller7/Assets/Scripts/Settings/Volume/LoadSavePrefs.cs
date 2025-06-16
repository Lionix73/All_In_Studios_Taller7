using UnityEngine;
using UnityEngine.UI;

public class LoadSavePrefs : MonoBehaviour
{
    private Slider slider;

    private void Awake()
    {
        slider = GetComponentInChildren<Slider>();
    }

    void Start()
    {
        LoadValue();

        slider.onValueChanged.AddListener(UpdateValue);
    }

    private void LoadValue()
    {
        float savedValue = PlayerPrefs.GetFloat(gameObject.name, 1f);
        slider.value = savedValue;
    }

    private void UpdateValue(float _)
    {
        PlayerPrefs.SetFloat(gameObject.name, slider.value);
    }
}
