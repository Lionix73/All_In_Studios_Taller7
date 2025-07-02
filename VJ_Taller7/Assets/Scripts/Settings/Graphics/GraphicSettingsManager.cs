using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraphicSettingsManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionsDropdown;
    [SerializeField] private Slider fpsSlider;
    [SerializeField] private Toggle fullscreenTogle;
    private Resolution[] resolutions;

    private const string RESOLUTION = "Resolution";
    private const string FULLSCREEN = "Fullscreen";
    private const string FPS = "targetFPS";

    public int ResolutionIndex { get; set; }
    public int Fullscreen { get; set; }
    public int TargetFPS { get; set; }

    #region Singleton
    private static GraphicSettingsManager _singleton;

    public static GraphicSettingsManager Singleton
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
    }
    #endregion

    private void Start()
    {
        SetResolutionOptions();
        LoadSavedData();
    }

    private void LoadSavedData()
    {
        ResolutionIndex = PlayerPrefs.GetInt(RESOLUTION, 19);
        Fullscreen = PlayerPrefs.GetInt(FULLSCREEN, 1);
        TargetFPS = PlayerPrefs.GetInt(FPS, 60);

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
        ResolutionIndex = currentRes;
    }

    public void SaveResolution(int resolutionIndex)
    {
        PlayerPrefs.SetInt(RESOLUTION, resolutionIndex);
        LoadSavedData();
    }

    public void SetResolution()
    {
        resolutionsDropdown.value = ResolutionIndex;
        resolutionsDropdown.RefreshShownValue();

        Resolution res = resolutions[ResolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
    #endregion

    #region -----FullScreen-----
    public void SaveFullscreen(bool isFullscreen)
    {
        PlayerPrefs.SetInt(FULLSCREEN, isFullscreen ? 1 : 0);
        LoadSavedData();
    }

    private void SetFullscreen()
    {
        bool isOn = Fullscreen == 1;
        fullscreenTogle.isOn = isOn;
        Screen.fullScreen = isOn;
    }
    #endregion

    #region -----FPS-----
    public void SaveTargetFPS(float fps)
    {
        PlayerPrefs.SetInt(FPS, (int)fps);
        LoadSavedData();
    }

    private void SetFPS()
    {
        fpsSlider.value = TargetFPS;
    }
    #endregion
}
