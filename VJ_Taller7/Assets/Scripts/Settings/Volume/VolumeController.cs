using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class VolumeController : MonoBehaviour
{
    private VCA vcaGeneral;
    private VCA vcaDialogues;
    private VCA vcaEnemies;
    private VCA vcaEnvironment;
    private VCA vcaGuns;
    private VCA vcaMusic;
    private VCA vcaPlayer;
    private VCA vcaUI;

    private const string PREF_GENERAL = "Volume_General";
    private const string PREF_DIALOGUES = "Volume_Dialogues";
    private const string PREF_ENEMIES = "Volume_Enemies";
    private const string PREF_ENVIRONMENT = "Volume_Environment";
    private const string PREF_GUNS = "Volume_Guns";
    private const string PREF_MUSIC = "Volume_Music";
    private const string PREF_PLAYER = "Volume_Player";
    private const string PREF_UI = "Volume_UI";

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

        // Aplicar valores guardados
        SetGeneralVolume(PlayerPrefs.GetFloat(PREF_GENERAL, 1f));
        SetDialoguesVolume(PlayerPrefs.GetFloat(PREF_DIALOGUES, 1f));
        SetEnemiesVolume(PlayerPrefs.GetFloat(PREF_ENEMIES, 1f));
        SetEnvironmentVolume(PlayerPrefs.GetFloat(PREF_ENVIRONMENT, 1f));
        SetGunsVolume(PlayerPrefs.GetFloat(PREF_GUNS, 1f));
        SetMusicVolume(PlayerPrefs.GetFloat(PREF_MUSIC, 1f));
        SetPlayerVolume(PlayerPrefs.GetFloat(PREF_PLAYER, 1f));
        SetUIVolume(PlayerPrefs.GetFloat(PREF_UI, 1f));
    }

    public void SetGeneralVolume(float value)
    {
        vcaGeneral.setVolume(value);
    }

    public void SetDialoguesVolume(float value)
    {
        vcaDialogues.setVolume(value);
    }

    public void SetEnemiesVolume(float value)
    {
        vcaEnemies.setVolume(value);
    }

    public void SetEnvironmentVolume(float value)
    {
        vcaEnvironment.setVolume(value);
    }

    public void SetGunsVolume(float value)
    {
        vcaGuns.setVolume(value);
    }

    public void SetMusicVolume(float value)
    {
        vcaMusic.setVolume(value);
    }

    public void SetPlayerVolume(float value)
    {
        vcaPlayer.setVolume(value);
    }

    public void SetUIVolume(float value)
    {
        vcaUI.setVolume(value);
    }

    // Asegura que los cambios se guarden en disco
    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}
