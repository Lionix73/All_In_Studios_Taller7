using Unity.Cinemachine;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using UnityEngine;
using UnityEngine.UI;

public class SensibilitySettings : MonoBehaviour
{
    private CinemachineInputAxisController axisController;
    private UIManager uiManager;

    private float lastSensiX;
    private float lastSensiY;

    private void Awake()
    {
        uiManager = UIManager.Singleton;
        axisController = GetComponent<CinemachineInputAxisController>();
    }

    private void Update()
    {
        if (lastSensiX == uiManager.SensibilityGainX && lastSensiY == uiManager.SensibilityGainY) return;
        
        AdjustSensibility();
    }

    public void AdjustSensibility()
    {
        lastSensiX = uiManager.SensibilityGainX;
        lastSensiY = uiManager.SensibilityGainY;

        foreach (InputAxisControllerBase<CinemachineInputAxisController.Reader>.Controller c in axisController.Controllers)
        {
            if (c.Name == "Look Orbit X")
            {
                c.Input.Gain = uiManager.SensibilityGainX;
                c.Input.LegacyGain = uiManager.SensibilityLegacyGainX;
            }
            else if (c.Name == "Look Orbit Y")
            {
                c.Input.Gain = uiManager.SensibilityGainY;
                c.Input.LegacyGain = uiManager.SensibilityLegacyGainY;
            }
        }
    }
}
