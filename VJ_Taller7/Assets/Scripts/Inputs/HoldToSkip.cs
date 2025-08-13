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
    [Tooltip("GameObject name of the obj with the image of the progress bar")]
    [SerializeField] private string progressBarName;

    [Header("Event")]
    public UnityEvent Skip;
    
    private float holdProgress = 0f;
    private bool isHolding = false;
    private Image progressBar;

    private void Awake()
    {
        progressBar = GameObject.Find(progressBarName).GetComponent<Image>();
    }

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
        if (progressBar.gameObject.activeSelf == false) return;

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
            progressBar.fillAmount = 0f;
            holdProgress = 0f;
            isHolding = false;
            Skip?.Invoke();
        }
    }
}
