using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;

public class HoldToSkip : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference holdAction;

    [Header("Settings")]
    [SerializeField] private float holdDuration = 1f; // segundos necesarios
    [SerializeField] private float decaySpeed = 1f;   // velocidad de descarga al soltar

    [Header("UI")]
    [SerializeField] private Image progressBar; // Usa fillAmount de 0 a 1

    [Header("Event")]
    public UnityEvent Skip;

    private float holdProgress = 0f;
    private bool isHolding = false;

    private void OnEnable()
    {
        if (holdAction != null)
        {
            holdAction.action.performed += OnHoldStarted;
            holdAction.action.canceled += OnHoldCanceled;
            holdAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (holdAction != null)
        {
            holdAction.action.performed -= OnHoldStarted;
            holdAction.action.canceled -= OnHoldCanceled;
            holdAction.action.Disable();
        }
    }

    private void OnHoldStarted(InputAction.CallbackContext ctx)
    {
        isHolding = true;
    }

    private void OnHoldCanceled(InputAction.CallbackContext ctx)
    {
        isHolding = false;
    }

    private void Update()
    {
        if (isHolding)
        {
            holdProgress += Time.deltaTime / holdDuration;
        }
        else
        {
            holdProgress -= Time.deltaTime * decaySpeed / holdDuration;
        }

        holdProgress = Mathf.Clamp01(holdProgress);

        if (progressBar != null)
        {
            progressBar.fillAmount = holdProgress;
        }

        if (holdProgress >= 1f)
        {
            Skip?.Invoke();
            enabled = false; // Desactivamos para evitar múltiples ejecuciones
            progressBar.fillAmount = 0f;
        }
    }
}
