using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class SensibilitySettings : MonoBehaviour
{
    private CinemachineInputAxisController axisController;
    private SensibilitySettingsManager SensibilitySettingsManager;

    private float currentSensiX;
    private float currentSensiY;

    private void Awake()
    {
        SensibilitySettingsManager = SensibilitySettingsManager.Singleton;
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
                c.Input.Gain = SensibilitySettingsManager.SensibilityGainX;
                c.Input.LegacyGain = SensibilitySettingsManager.SensibilityLegacyGainX;
            }
            else if (c.Name == "Look Orbit Y")
            {
                c.Input.Gain = SensibilitySettingsManager.SensibilityGainY;
                c.Input.LegacyGain = SensibilitySettingsManager.SensibilityLegacyGainY;
            }
        }
    }

    public void AdjustSensiDuringAim()
    {
        SensibilitySettingsManager.SensibilityGainX *= SensibilitySettingsManager.AimSensiMultiplier;
        SensibilitySettingsManager.SensibilityGainY *= SensibilitySettingsManager.AimSensiMultiplier;
        ChangeCameraSensitivity(1);
    }

    public void AdjustSensiNoAim()
    {
        SensibilitySettingsManager.SensibilityGainX = currentSensiX;
        SensibilitySettingsManager.SensibilityGainY = currentSensiY;
        ChangeCameraSensitivity(1);
    }

    private void GetCurrentSensitivity(float _)
    {
        currentSensiX = SensibilitySettingsManager.SensibilityGainX;
        currentSensiY = SensibilitySettingsManager.SensibilityGainY;
    }
}
