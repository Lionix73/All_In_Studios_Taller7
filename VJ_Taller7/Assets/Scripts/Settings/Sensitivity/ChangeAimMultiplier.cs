using UnityEngine;
using UnityEngine.UI;

public class ChangeAimMultiplier : MonoBehaviour
{
    private Slider slider;
    private SettingsManager settingsManager;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        settingsManager = SettingsManager.Singleton;
    }

    private void Start()
    {
        slider.value = settingsManager.defaultAimMultiplier;
    }

    public void ChangeAimSensitivity()
    {
        if (settingsManager != null)
        {
            settingsManager.AimSensiMultiplier = slider.value;
        }
    }
}
