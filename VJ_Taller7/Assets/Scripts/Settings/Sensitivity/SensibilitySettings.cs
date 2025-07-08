using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SensibilitySettings : MonoBehaviour
{
    [Header("Input Actions")]
    [Tooltip("Choose what action will pause the camera")]
    [SerializeField] private InputActionReference emotes;
    
    private CinemachineInputAxisController axisController;
    private SensibilitySettingsManager SensibilitySettingsManager;

    private float currentSensiX;
    private float currentSensiY;

    private bool cameraFreeze;

    private void Awake()
    {
        SensibilitySettingsManager = SensibilitySettingsManager.Instance;
        axisController = GetComponent<CinemachineInputAxisController>();
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

    private void EmotesPauseCamera(InputAction.CallbackContext _)
    {
        PauseCamera();
    }

    public void PauseCamera()
    {
        if (!cameraFreeze)
        {
            foreach (InputAxisControllerBase<CinemachineInputAxisController.Reader>.Controller c in axisController.Controllers)
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

        cameraFreeze = !cameraFreeze;
    }

    public void ChangeCameraSensitivity(float _)
    {
        foreach (InputAxisControllerBase<CinemachineInputAxisController.Reader>.Controller c in axisController.Controllers)
        {
            if (c.Name == "Look Orbit X")
            {
                c.Input.Gain = SensibilitySettingsManager.SensibilityGainX;
                c.Input.LegacyGain = SensibilitySettingsManager.SensibilityLegacyGainX;
            }
            else if (c.Name == "Look Orbit Y")
            {
                c.Input.Gain = SensibilitySettingsManager.SensibilityGainY;
                c.Input.LegacyGain = SensibilitySettingsManager.SensibilityLegacyGainY;
            }
        }
    }

    public void AdjustSensiDuringAim()
    {
        SensibilitySettingsManager.SensibilityGainX *= SensibilitySettingsManager.AimSensiMultiplier;
        SensibilitySettingsManager.SensibilityGainY *= SensibilitySettingsManager.AimSensiMultiplier;
        ChangeCameraSensitivity(1);
    }

    public void AdjustSensiNoAim()
    {
        SensibilitySettingsManager.SensibilityGainX = currentSensiX;
        SensibilitySettingsManager.SensibilityGainY = currentSensiY;
        ChangeCameraSensitivity(1);
    }

    private void GetCurrentSensitivity(float _)
    {
        currentSensiX = SensibilitySettingsManager.SensibilityGainX;
        currentSensiY = SensibilitySettingsManager.SensibilityGainY;
    }
}
