using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    [SerializeField] private SettingsSO settingsSO;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = settingsSO.TargetFPS;
    }   
}
