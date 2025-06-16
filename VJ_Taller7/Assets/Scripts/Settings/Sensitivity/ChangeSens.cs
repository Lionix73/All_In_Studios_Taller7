using UnityEngine;
using UnityEngine.UI;

public class ChangeSens : MonoBehaviour
{
    [SerializeField] private bool vertical;
    
    private Slider slider;
    private SensibilitySettingsManager sensiManager;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        sensiManager = SensibilitySettingsManager.Singleton;
    }

    private void Start()
    {
        if (vertical)
        {
            slider.value = Mathf.Abs(sensiManager.SensibilityGainY);
        }
        else
            slider.value = sensiManager.SensibilityGainX;
    }

    public void ChangeSensGain()
    {
        if (!vertical)
        {
            if (sensiManager != null)
            {
                float newValue = slider.value;

                sensiManager.SensibilityGainX = newValue;
                sensiManager.SensibilityLegacyGainX = newValue * 100;
                PlayerPrefs.SetFloat("Sensibility_Horizontal", newValue);
            }
        }
        else if (vertical)
        {
            if (sensiManager != null)
            {
                float newValue = slider.value * -1;

                sensiManager.SensibilityGainY = newValue;
                sensiManager.SensibilityLegacyGainY = newValue * 100;
                PlayerPrefs.SetFloat("Sensibility_Vertical", newValue);
            }
        }
    }
}
