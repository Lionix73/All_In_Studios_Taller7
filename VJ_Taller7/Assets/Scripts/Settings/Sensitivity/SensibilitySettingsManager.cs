using UnityEngine;

[DefaultExecutionOrder(-1)]
public class SensibilitySettingsManager : MonoBehaviour
{
    public float defaultSensiX = 5f;
    public float defaultSensiY = -2f;
    public float defaultAimMultiplier = 0.8f;

    public float SensibilityGainX { get; set; }
    public float SensibilityLegacyGainX { get; set; }
    public float SensibilityGainY { get; set; }
    public float SensibilityLegacyGainY { get; set; }
    public float AimSensiMultiplier { get; set; }

    private static SensibilitySettingsManager _singleton;

    public static SensibilitySettingsManager Singleton
    {
        get => _singleton;
        set => _singleton = value;
    }

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

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

    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;
    }
}
