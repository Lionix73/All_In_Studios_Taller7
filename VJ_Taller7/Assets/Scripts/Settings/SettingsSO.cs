using UnityEngine;

[CreateAssetMenu(fileName = "SettingsSO", menuName = "Scriptable Objects/SettingsSO")]
public class SettingsSO : ScriptableObject
{
    [Header("Sensitivity Settings")]
    public float SensibilityGainX = 5f;
    public float SensibilityGainY = -3f;
    [Range(0, 1)] public float AimSensiMultiplier = 0.8f; // Reduced sensitivity when aiming

    [Space(10)]
    [Header("Volume Settings")]
    [Range(0, 2)] public float GeneralVolume = 1f;
    [Range(0, 2)] public float DialoguesVolume = 1f;
    [Range(0, 2)] public float EnemiesVolume = 1f;
    [Range(0, 2)] public float EnvironmentVolume = 1f;
    [Range(0, 2)] public float GunsVolume = 1f;
    [Range(0, 2)] public float MusicVolume = 1f;
    [Range(0, 2)] public float PlayerVolume = 1f;
    [Range(0, 2)] public float UIVolume = 1f;

    [Space(10)]
    [Header("Graphics Settings")]
    public int ResolutionIndex = 19; // Default to 1920x1080
    public int TargetFPS = 60;
    public bool Fullscreen = true;
}
