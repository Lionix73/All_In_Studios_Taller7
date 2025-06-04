using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class SensibilitySettings : MonoBehaviour
{
    private CinemachineInputAxisController axisController;
    private SettingsManager settingsManager;

    private void Awake()
    {
        settingsManager = SettingsManager.Singleton;
        axisController = GetComponent<CinemachineInputAxisController>();
    }

    private void Start()
    {
        AdjustSensibility(1);

        foreach (var item in FindObjectsByType<ChangeSens>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Slider s = item.gameObject.GetComponent<Slider>();
            s.onValueChanged.AddListener(AdjustSensibility);
        }
    }

    public void AdjustSensibility(float _)
    {
        foreach (InputAxisControllerBase<CinemachineInputAxisController.Reader>.Controller c in axisController.Controllers)
        {
            if (c.Name == "Look Orbit X")
            {
                c.Input.Gain = settingsManager.SensibilityGainX;
                c.Input.LegacyGain = settingsManager.SensibilityLegacyGainX;
            }
            else if (c.Name == "Look Orbit Y")
            {
                c.Input.Gain = settingsManager.SensibilityGainY;
                c.Input.LegacyGain = settingsManager.SensibilityLegacyGainY;
            }
        }
    }
}
