using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraphicSettingsManager : Singleton<GraphicSettingsManager>
{
    [SerializeField] private SettingsSO settingsSO;

    [SerializeField] private TMP_Dropdown resolutionsDropdown;
    [SerializeField] private Slider fpsSlider;
    [SerializeField] private Toggle fullscreenTogle;
    private Resolution[] resolutions;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        SetResolutionOptions();
        LoadSavedData();
    }

    private void LoadSavedData()
    {
        SetResolution();
        SetFullscreen();
        SetFPS();
    }

    #region -----Resolution-----
    private void SetResolutionOptions()
    {
        resolutions = Screen.resolutions;
        resolutionsDropdown.ClearOptions();

        List<string> options = new();

        int currentRes = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string op = $"{resolutions[i].width} x {resolutions[i].height}";
            options.Add(op);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                currentRes = i;
        }

        resolutionsDropdown.AddOptions(options);
        settingsSO.ResolutionIndex = currentRes;
    }

    public void SaveResolution(int resolutionIndex)
    {
        settingsSO.ResolutionIndex = resolutionIndex;
        LoadSavedData();
    }

    public void SetResolution()
    {
        resolutionsDropdown.value = settingsSO.ResolutionIndex;
        resolutionsDropdown.RefreshShownValue();

        Resolution res = resolutions[settingsSO.ResolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
    #endregion

    #region -----FullScreen-----
    public void SaveFullscreen(bool isFullscreen)
    {
        settingsSO.Fullscreen = isFullscreen;
        LoadSavedData();
    }

    private void SetFullscreen()
    {
        bool isOn = settingsSO.Fullscreen;
        fullscreenTogle.isOn = isOn;
        Screen.fullScreen = isOn;
    }
    #endregion

    #region -----FPS-----
    public void SaveTargetFPS(float fps)
    {
        settingsSO.TargetFPS = Mathf.RoundToInt(fps);
        LoadSavedData();
    }

    private void SetFPS()
    {
        fpsSlider.value = settingsSO.TargetFPS;
    }
    #endregion
}
