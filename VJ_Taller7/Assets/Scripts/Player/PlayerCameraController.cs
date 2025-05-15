using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    public static PlayerCameraController Instance { get; private set; }
    [SerializeField] private InputActionReference lookAction; // Asigna el Input Action "Look" aquí

    public float mouseSensitivity = 2f;
    private Vector2 mouseSens;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Carga la sensibilidad guardada
        if (GameSettingsManager.Instance != null)
        {
            mouseSensitivity = GameSettingsManager.Instance.MouseSensitivity;
        }
        else
        {
            mouseSens.x = PlayerPrefs.GetFloat("MouseSensitivity");
            mouseSens.y = PlayerPrefs.GetFloat("MouseSensitivity");

        }
    }

    // Método para actualizar sensibilidad desde el GameSettingsManager
    public void UpdateSensitivity(float newSensitivity)
    {
        mouseSensitivity = newSensitivity;


    }
}