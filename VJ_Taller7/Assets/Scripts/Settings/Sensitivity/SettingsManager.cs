using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public float defaultSensiX = 5f;
    public float defaultSensiY = -2f;
    public float defaultAimMultiplier = 0.8f;

    public float SensibilityGainX { get; set; }
    public float SensibilityLegacyGainX { get; set; }
    public float SensibilityGainY { get; set; }
    public float SensibilityLegacyGainY { get; set; }
    public float AimSensiMultiplier { get; set; }

    private static SettingsManager _singleton;

    public static SettingsManager Singleton
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

        SensibilityGainX = defaultSensiX;
        SensibilityGainY = defaultSensiY;
        AimSensiMultiplier = defaultAimMultiplier;

        SensibilityLegacyGainX = defaultSensiX * 100f;
        SensibilityLegacyGainY = defaultSensiY * 100f;
    }

    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;
    }
}
