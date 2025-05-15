using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance { get; private set; }
    [SerializeField] private InputActionReference lookAction; // Asigna el Input Action "Look" aquí

    [SerializeField] private Slider sensitivitySlider;
    public float MouseSensitivity { get; private set; } = 2f; // Valor por defecto

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre escenas
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void SetupSensitivitySlider()
    {
        // Configura el slider (si está asignado)
        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = 0.1f;
            sensitivitySlider.maxValue = 2f;
            sensitivitySlider.value = MouseSensitivity;
            sensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);
        }
    }
    public void UpdateSensitivity(float newValue)
    {
        MouseSensitivity = newValue;
        PlayerPrefs.SetFloat("MouseSensitivity", MouseSensitivity);
        PlayerPrefs.Save();

        // Si hay una cámara activa, actualiza su sensibilidad
        if (PlayerCameraController.Instance != null)
        {
            PlayerCameraController.Instance.UpdateSensitivity(MouseSensitivity);
        }
    }
}