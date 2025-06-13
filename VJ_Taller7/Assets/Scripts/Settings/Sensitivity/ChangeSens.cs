using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.UI;

public class ChangeSens : MonoBehaviour
{
    [SerializeField] private bool vertical;
    
    private Slider slider;
    private SettingsManager settingsManager;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        settingsManager = SettingsManager.Singleton;
    }

    private void Start()
    {
        if (vertical)
            slider.value = Mathf.Abs(settingsManager.defaultSensiY);
        else
            slider.value = settingsManager.defaultSensiX;
    }

    public void ChangeSensGain()
    {
        if (!vertical)
        {
            if (settingsManager != null)
            {
                settingsManager.SensibilityGainX = slider.value;
                settingsManager.SensibilityLegacyGainX = slider.value * 100;
            }
        }
        else if (vertical)
        {
            if (settingsManager != null)
            {
                settingsManager.SensibilityGainY = slider.value * -1;
                settingsManager.SensibilityLegacyGainY = slider.value * -100;
            }
        }
    }
}
