using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public float defaultSensiX = 5f;
    public float defaultSensiY = 2f;

    public float SensibilityGainX { get; set; }
    public float SensibilityLegacyGainX { get; set; }
    public float SensibilityGainY { get; set; }
    public float SensibilityLegacyGainY { get; set; }

    private static SettingsManager _singleton;

    public static SettingsManager Singleton
    {
        get => _singleton;

        set
        {
            if (_singleton == null)
            {
                _singleton = value;
                DontDestroyOnLoad(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;

        SensibilityGainX = defaultSensiX;
        SensibilityGainY = defaultSensiY;

        SensibilityLegacyGainX = defaultSensiX * 100f;
        SensibilityLegacyGainY = defaultSensiY * 100f;
    }

    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;
    }
}
