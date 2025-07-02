using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsSchemeManager : MonoBehaviour
{
    #region Controls Scheme
    public enum InputScheme { KeyboardMouse, Gamepad }

    [SerializeField] private InputScheme currentScheme = InputScheme.KeyboardMouse;
    public InputScheme ChangeScheme 
    { 
        get => currentScheme;
        set => currentScheme = value;
    }
    #endregion

    PlayerInput playerInput;
    public PlayerInput GetPlayerInput
    {
        get => playerInput;
    }

    public delegate void ControlsChange();
    public event ControlsChange OnControlsChange;

    #region Singleton
    private static ControlsSchemeManager _singleton;
    public static ControlsSchemeManager Singleton
    {
        get => _singleton;
        set => _singleton = value;
    }

    private void Awake()
    {
        playerInput = transform.root.GetComponentInChildren<PlayerInput>();

        if (Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    #endregion

    void Start()
    {
        if (playerInput != null)
        {
            playerInput.controlsChangedEvent.AddListener(OnControlsChanged);
        }
    }

    void OnControlsChanged(PlayerInput playerInput)
    {
        if (playerInput.currentControlScheme != null)
        {
            //Debug.Log("Current control scheme: " + playerInput.currentControlScheme);
            UpdateScheme(playerInput);
            OnControlsChange?.Invoke();
        }
    }

    public void UpdateScheme(PlayerInput scheme)
    {
        switch (scheme.currentControlScheme)
        {
            case "Keyboard&Mouse":
                ChangeScheme = InputScheme.KeyboardMouse;
                break;
            case "Gamepad":
                ChangeScheme = InputScheme.Gamepad;
                break;
        }
    }
}
