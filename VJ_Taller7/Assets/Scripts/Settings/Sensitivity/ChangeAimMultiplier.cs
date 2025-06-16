using UnityEngine;
using UnityEngine.UI;

public class ChangeAimMultiplier : MonoBehaviour
{
    private Slider slider;
    private SensibilitySettingsManager sensiManager;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        sensiManager = SensibilitySettingsManager.Singleton;
    }

    private void Start()
    {
        slider.value = sensiManager.AimSensiMultiplier;
    }

    public void ChangeAimSensitivity()
    {
        if (sensiManager != null)
        {
            float newValue = slider.value;

            sensiManager.AimSensiMultiplier = newValue;
            PlayerPrefs.SetFloat("Sensibility_AimMultiplier", newValue); 
        }
    }
}
