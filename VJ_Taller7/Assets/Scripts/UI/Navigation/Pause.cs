using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject objectToSelect;

    [Header("Input Actions")]
    [Tooltip("Choose what action will activate o deactivate the Pause")]
    [SerializeField] private InputActionReference actionReference;

    private UIManager uiManager;
    private ThisObjectSounds _soundManager;

    private void Start()
    {
        uiManager = UIManager.Singleton;
        _soundManager = GetComponentInChildren<ThisObjectSounds>();
    }

    private void OnEnable()
    {
        if (actionReference != null && actionReference.action != null)
        {
            actionReference.action.performed += OnActionCalled;
            actionReference.action.Enable();
        }
        else
        {
            Debug.LogError("No se ha asignado la acción correctamente.");
        }
    }

    private void OnDisable()
    {
        if (actionReference != null && actionReference.action != null)
        {
            actionReference.action.performed -= OnActionCalled;
            actionReference.action.Disable();
        }
    }

    private void OnActionCalled(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            if (!uiManager.IsPaused)
            {
                _soundManager.PlaySound("PausedButton");
            }
            else
            {
                _soundManager.PlaySound("PlayButton");
            }

            uiManager.PauseGame(4);
            EventSystem.current.currentSelectedGameObject = objectToSelect;
        }
    }
}