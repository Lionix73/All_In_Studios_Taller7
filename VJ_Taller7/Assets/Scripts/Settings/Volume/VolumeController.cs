using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.UI;
using Unity.Mathematics;

public class VolumeController : MonoBehaviour
{
    [SerializeField] private SettingsSO settings;

    [SerializeField] private Slider generalSlider;
    [SerializeField] private Slider dialoguesSlider;
    [SerializeField] private Slider enemiesSlider;
    [SerializeField] private Slider environmentSlider;
    [SerializeField] private Slider gunsSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider playerSlider;
    [SerializeField] private Slider uiSlider;

    private VCA vcaGeneral;
    private VCA vcaDialogues;
    private VCA vcaEnemies;
    private VCA vcaEnvironment;
    private VCA vcaGuns;
    private VCA vcaMusic;
    private VCA vcaPlayer;
    private VCA vcaUI;

    void Start()
    {
        vcaGeneral = RuntimeManager.GetVCA("vca:/VCA_General");
        vcaDialogues = RuntimeManager.GetVCA("vca:/VCA_Dialogues");
        vcaEnemies = RuntimeManager.GetVCA("vca:/VCA_Enemies");
        vcaEnvironment = RuntimeManager.GetVCA("vca:/VCA_Environment");
        vcaGuns = RuntimeManager.GetVCA("vca:/VCA_Guns");
        vcaMusic = RuntimeManager.GetVCA("vca:/VCA_Music");
        vcaPlayer = RuntimeManager.GetVCA("vca:/VCA_Player");
        vcaUI = RuntimeManager.GetVCA("vca:/VCA_UI");

        SetGeneralVolume(settings.GeneralVolume);
        SetDialoguesVolume(settings.DialoguesVolume);
        SetEnemiesVolume(settings.EnemiesVolume);
        SetEnvironmentVolume(settings.EnvironmentVolume);
        SetGunsVolume(settings.GunsVolume);
        SetMusicVolume(settings.MusicVolume);
        SetPlayerVolume(settings.PlayerVolume);
        SetUIVolume(settings.UIVolume);
    }

    public void SetGeneralVolume(float value)
    {
        settings.GeneralVolume = value;
        vcaGeneral.setVolume(value);
    }

    public void SetDialoguesVolume(float value)
    {
        settings.DialoguesVolume = value;
        vcaDialogues.setVolume(value);
    }

    public void SetEnemiesVolume(float value)
    {
        settings.EnemiesVolume = value;
        vcaEnemies.setVolume(value);
    }

    public void SetEnvironmentVolume(float value)
    {
        settings.EnvironmentVolume = value;
        vcaEnvironment.setVolume(value);
    }

    public void SetGunsVolume(float value)
    {
        settings.GunsVolume = value;
        vcaGuns.setVolume(value);
    }

    public void SetMusicVolume(float value)
    {
        settings.MusicVolume = value;
        vcaMusic.setVolume(value);
    }

    public void SetPlayerVolume(float value)
    {
        settings.PlayerVolume = value;
        vcaPlayer.setVolume(value);
    }

    public void SetUIVolume(float value)
    {
        settings.UIVolume = value;
        vcaUI.setVolume(value);
    }

    public float GetVolume(string channel)
    {
        switch (channel)
        {
            case "Volume_General":
                return settings.GeneralVolume;
            case "Volume_Dialogues":
                return settings.DialoguesVolume;
            case "Volume_Enemies":
                return settings.EnemiesVolume;
            case "Volume_Environment":
                return settings.EnvironmentVolume;
            case "Volume_Guns":
                return settings.GunsVolume;
            case "Volume_Music":
                return settings.MusicVolume;
            case "Volume_Player":
                return settings.PlayerVolume;
            case "Volume_UI":
                return settings.UIVolume;
            default:
                return 0f;
        }
    }
}
    