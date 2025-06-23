using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// This script is used to link an Action of the InputActions to the click event of a button
public class LinkActionToButton : MonoBehaviour
{
    [Header("Input Actions")]
    [Tooltip("Choose what action will trigger the OnClick Event")]
    [SerializeField] private InputActionReference actionReference;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
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
        button.onClick.Invoke();
    }
}
