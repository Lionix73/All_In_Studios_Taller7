using UnityEngine;

[DefaultExecutionOrder(-1)]
public class SensibilitySettingsManager : PersistentSingleton<SensibilitySettingsManager>
{
    [Header("Current Values")]
    [field: SerializeField] public float SensibilityGainX { get; set; }
    public float SensibilityLegacyGainX { get; set; }
    [field:SerializeField] public float SensibilityGainY { get; set; }
    public float SensibilityLegacyGainY { get; set; }
    [field:SerializeField] public float AimSensiMultiplier { get; set; }

    [Header("Default Values")]
    [SerializeField] private float defaultSensiX = 5f;
    [SerializeField] private float defaultSensiY = -2f;
    [SerializeField] private float defaultAimMultiplier = 0.8f;
    
    protected override void Awake()
    {
        base.Awake();

        SetSensibility();
    }

    private void SetSensibility()
    {
        float sensibilityX = PlayerPrefs.GetFloat("Sensibility_Horizontal", defaultSensiX);
        float sensibilityY = PlayerPrefs.GetFloat("Sensibility_Vertical", defaultSensiY);
        float aimMulti = PlayerPrefs.GetFloat("Sensibility_AimMultiplier", defaultAimMultiplier);

        SensibilityGainX = sensibilityX;
        SensibilityGainY = sensibilityY;
        AimSensiMultiplier = aimMulti;

        SensibilityLegacyGainX = sensibilityX * 100f;
        SensibilityLegacyGainY = sensibilityY * 100f;
    }
}
