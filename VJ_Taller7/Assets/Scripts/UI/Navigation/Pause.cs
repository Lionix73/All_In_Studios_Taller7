using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{
    [Tooltip("Choose what object will be selected after pause")]
    [SerializeField] private GameObject objectToSelect;

    [Header("Input Actions")]
    [Tooltip("Choose what action will activate o deactivate the Pause")]
    [SerializeField] private InputActionReference actionReference;

    private UIManager _uiManager;
    private PlayerController _playerController;
    private ThisObjectSounds _soundManager;

    private void Start()
    {
        _uiManager = UIManager.Singleton;
        _soundManager = GetComponentInChildren<ThisObjectSounds>();
    }

    #region -----Link Action------
    private void OnEnable()
    {
        if (actionReference != null && actionReference.action != null)
        {
            actionReference.action.performed += OnActionCalled;
            actionReference.action.Enable();
        }
        else
        {
            Debug.LogError("No se ha asignado la accion correctamente");
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
        if (_uiManager.IsMainMenu) return;

        if (context.performed)
        {
            if (!_uiManager.IsPaused) PauseGame();
        }
    }
    #endregion

    #region -----Pause Game------
    public void PauseGame()
    {
        _soundManager.PlaySound("PausedButton");

        if (_playerController == null)
            _playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        _playerController.BlockMovement();
        _uiManager.PauseGame(4);

        EventSystem.current.currentSelectedGameObject = objectToSelect;
    }
    #endregion

    #region -----Resume Game------
    public void UnPauseGame()
    {
        StartCoroutine(DelayResume());
    }

    private IEnumerator DelayResume()
    {
        _soundManager.PlaySound("PlayButton");
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        yield return new WaitForSeconds(0.1f);
        _playerController.BlockMovement();
        _uiManager.PauseGame(4);
    }
    #endregion
}