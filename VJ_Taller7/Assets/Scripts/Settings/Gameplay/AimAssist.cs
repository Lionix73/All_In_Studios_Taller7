using UnityEngine;

public class AimAssist : MonoBehaviour
{
    [Header("Aim Assist Settings")]
    [SerializeField] private bool enableAimAssist = true;
    [Tooltip("Radius of the sticky aim area")]
    [SerializeField] private float stickyAimRadius = 0.5f;
    [Tooltip("How far the aim assist will work")]
    [SerializeField] private float stickyAimDistance = 100f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Dynamic Assist Settings")]
    [Tooltip("Multiplier for aim assist when the enemy is close")]
    [SerializeField][Range(0, 1)] private float maxAssistMultiplier = 0.8f; // cuando el enemigo está cerca
    [Tooltip("Multiplier for aim assist when the enemy is at the edge of the range")]
    [SerializeField][Range(0, 1)] private float minAssistMultiplier = 0.2f; // cuando el enemigo está al borde del alcance

    private SensibilitySettings _sensibilitySettings;
    private PlayerController _playerController;
    private Camera _mainCam;
    private bool _isAssistActive = false;
    private float _lastAssistMultiplier = -1f;

    private void Start()
    {
        _mainCam = Camera.main;
        _sensibilitySettings = FindFirstObjectByType<SensibilitySettings>();
        _playerController = transform.root.GetComponentInChildren<PlayerController>();
    }

    private void Update()
    {
        if (!enableAimAssist) return;

        if (_playerController.PlayerIsAiming)
        {
            OnAimingAssist();
        }
        else
        {
            _isAssistActive = false;
            _lastAssistMultiplier = -1f;
        }
    }

    private void OnAimingAssist()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f);
        Ray ray = _mainCam.ScreenPointToRay(screenCenter);

        if (Physics.SphereCast(ray, stickyAimRadius, out RaycastHit hit, stickyAimDistance, enemyLayer))
        {
            // Cálculo dinámico según distancia
            float t = Mathf.InverseLerp(stickyAimDistance, 0f, hit.distance); // a menor distancia, mayor t
            float dynamicMultiplier = Mathf.Lerp(minAssistMultiplier, maxAssistMultiplier, t);

            if (!_isAssistActive || !Mathf.Approximately(dynamicMultiplier, _lastAssistMultiplier))
            {
                _sensibilitySettings.AdjustSensiAimAssit(dynamicMultiplier);
                _isAssistActive = true;
                _lastAssistMultiplier = dynamicMultiplier;
            }
        }
        else if (_isAssistActive)
        {
            _sensibilitySettings.AdjustSensiDuringAim(); // vuelve a sensibilidad de apuntado
            _isAssistActive = false;
            _lastAssistMultiplier = -1f;
        }
    }

    public void SetAimAssistActive(bool state)
    {
        enableAimAssist = state;
        if (!state && _isAssistActive)
        {
            _sensibilitySettings.AdjustSensiDuringAim();
            _isAssistActive = false;
        }
    }
}
