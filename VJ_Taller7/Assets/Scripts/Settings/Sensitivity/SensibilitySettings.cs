using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class SensibilitySettings : MonoBehaviour
{
    private CinemachineInputAxisController axisController;
    private SettingsManager settingsManager;

    private float currentSensiX;
    private float currentSensiY;

    private void Awake()
    {
        settingsManager = SettingsManager.Singleton;
        axisController = GetComponent<CinemachineInputAxisController>();
    }

    private void Start()
    {
        ChangeCameraSensitivity(1);
        GetCurrentSensitivity(1);

        foreach (var item in FindObjectsByType<ChangeSens>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Slider s = item.gameObject.GetComponent<Slider>();
            s.onValueChanged.AddListener(ChangeCameraSensitivity);
            s.onValueChanged.AddListener(GetCurrentSensitivity);
        }
    }

    public void ChangeCameraSensitivity(float _)
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

    public void AdjustSensiDuringAim()
    {
        settingsManager.SensibilityGainX *= settingsManager.AimSensiMultiplier;
        settingsManager.SensibilityGainY *= settingsManager.AimSensiMultiplier;
        ChangeCameraSensitivity(1);
    }

    public void AdjustSensiNoAim()
    {
        settingsManager.SensibilityGainX = currentSensiX;
        settingsManager.SensibilityGainY = currentSensiY;
        ChangeCameraSensitivity(1);
    }

    private void GetCurrentSensitivity(float _)
    {
        currentSensiX = settingsManager.SensibilityGainX;
        currentSensiY = settingsManager.SensibilityGainY;
    }
}
