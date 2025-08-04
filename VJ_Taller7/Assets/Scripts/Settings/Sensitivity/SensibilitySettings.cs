using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SensibilitySettings : MonoBehaviour
{
    [Header("Settings Manager")]
    [ExposedScriptableObject]
    [SerializeField] private SettingsSO _sensibilitySettingsManager;

    [Header("Input Actions")]
    [Tooltip("Choose what action will pause the camera")]
    [SerializeField] private InputActionReference emotes;

    private CinemachineInputAxisController _axisController;

    private float _currentSensiX;
    private float _currentSensiY;
    private bool _cameraFreeze;

    private void Awake()
    {
        _axisController = GetComponent<CinemachineInputAxisController>();
    }

    private void Start()
    {
        ChangeCameraSensitivity(1);
        GetCurrentSensitivity(1);

        foreach (var item in FindObjectsByType<ChangeSens>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Slider s = item.gameObject.GetComponent<Slider>();
            s.onValueChanged.AddListener(ChangeCameraSensitivity);
            s.onValueChanged.AddListener(GetCurrentSensitivity);
        }
    }

    private void OnEnable()
    {
        if (emotes != null && emotes.action != null)
        {
            emotes.action.started += EmotesPauseCamera;
            emotes.action.canceled += EmotesPauseCamera;
            emotes.action.Enable();
        }
        else
        {
            Debug.LogError("No se ha asignado la accion correctamente.");
        }
    }

    private void OnDisable()
    {
        if (emotes != null && emotes.action != null)
        {
            emotes.action.started -= EmotesPauseCamera;
            emotes.action.canceled -= EmotesPauseCamera;
            emotes.action.Disable();
        }
    }

    #region -----Pause Camera-----
    private void EmotesPauseCamera(InputAction.CallbackContext _)
    {
        PauseCamera();
    }

    public void PauseCamera()
    {
        if (!_cameraFreeze)
        {
            foreach (InputAxisControllerBase<CinemachineInputAxisController.Reader>.Controller c in _axisController.Controllers)
            {
                if (c.Name == "Look Orbit X")
                {
                    c.Input.Gain = 0f;
                    c.Input.LegacyGain = 0f;
                }
                else if (c.Name == "Look Orbit Y")
                {
                    c.Input.Gain = 0f;
                    c.Input.LegacyGain = 0f;
                }
            }
        }
        else
        {
            ChangeCameraSensitivity(1);
        }

        _cameraFreeze = !_cameraFreeze;
    }
    #endregion

    #region -----Aim Sensitivity Adjustment Methods-----
    public void ChangeCameraSensitivity(float _)
    {
        foreach (InputAxisControllerBase<CinemachineInputAxisController.Reader>.Controller c in _axisController.Controllers)
        {
            if (c.Name == "Look Orbit X")
            {
                c.Input.Gain = _sensibilitySettingsManager.SensibilityGainX;
            }
            else if (c.Name == "Look Orbit Y")
            {
                c.Input.Gain = _sensibilitySettingsManager.SensibilityGainY;
            }
        }
    }

    public void AdjustSensiNoAim()
    {
        _sensibilitySettingsManager.SensibilityGainX = _currentSensiX;
        _sensibilitySettingsManager.SensibilityGainY = _currentSensiY;
        ChangeCameraSensitivity(1);
    }

    public void AdjustSensiDuringAim()
    {
        _sensibilitySettingsManager.SensibilityGainX = _currentSensiX * _sensibilitySettingsManager.AimSensiMultiplier;
        _sensibilitySettingsManager.SensibilityGainY = _currentSensiY * _sensibilitySettingsManager.AimSensiMultiplier;
        ChangeCameraSensitivity(1);
    }


    public void AdjustSensiAimAssit(float stickyMultiplier)
    {
        _sensibilitySettingsManager.SensibilityGainX = _currentSensiX * _sensibilitySettingsManager.AimSensiMultiplier * stickyMultiplier;
        _sensibilitySettingsManager.SensibilityGainY = _currentSensiY * _sensibilitySettingsManager.AimSensiMultiplier * stickyMultiplier;
        ChangeCameraSensitivity(1);
    }
    #endregion

    private void GetCurrentSensitivity(float _)
    {
        _currentSensiX = _sensibilitySettingsManager.SensibilityGainX;
        _currentSensiY = _sensibilitySettingsManager.SensibilityGainY;
    }
}
