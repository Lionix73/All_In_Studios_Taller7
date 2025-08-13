using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class HoldToEvent : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference holdAction;

    [Header("Settings")]
    [SerializeField] private float holdDuration = 1f; // segundos necesarios
    [SerializeField] private float decaySpeed = 1f;   // velocidad de descarga al soltar

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
        }
    }

    private void OnDisable()
    {
        if (holdAction != null)
        {
            holdAction.action.performed -= OnHoldStarted;
            holdAction.action.canceled -= OnHoldCanceled;
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

        if (holdProgress >= 1f)
        {
            Skip?.Invoke();
        }
    }
}
