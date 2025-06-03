using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.UI;

public class ChangeSens : MonoBehaviour
{
    [SerializeField] private bool vertical;
    
    private Slider slider;
    private UIManager uiManager;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        uiManager = UIManager.Singleton;
    }

    public void ChangeSensGain()
    {
        if (!vertical)
        {
            if (uiManager != null)
            {
                uiManager.SensibilityGainX = slider.value;
                uiManager.SensibilityLegacyGainX = slider.value * 100;
            }
        }
        else if (vertical)
        {
            if (uiManager != null)
            {
                uiManager.SensibilityGainY = slider.value * -1;
                uiManager.SensibilityLegacyGainY = slider.value * -100;
            }
        }
    }
}
