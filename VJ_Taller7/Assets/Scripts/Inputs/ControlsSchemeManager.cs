using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsSchemeManager : Singleton<ControlsSchemeManager>
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

    private PlayerInput playerInput;
    public PlayerInput GetPlayerInput { get => playerInput; }

    private GamepadVibration gamepadVibration;

    public delegate void ControlsChange();
    public event ControlsChange OnControlsChange;

    protected override void Awake()
    {
        base.Awake();
        
        playerInput = transform.root.GetComponentInChildren<PlayerInput>();
        gamepadVibration = GetComponent<GamepadVibration>();
    }

    private void Start()
    {
        if (playerInput != null)
        {
            playerInput.controlsChangedEvent.AddListener(OnControlsChanged);
            UpdateScheme(playerInput);
        }
    }

    private void OnControlsChanged(PlayerInput playerInput)
    {
        if (playerInput.currentControlScheme != null)
        {
            UpdateScheme(playerInput);
            OnControlsChange?.Invoke();
        }
    }

    private void UpdateScheme(PlayerInput scheme)
    {
        switch (scheme.currentControlScheme)
        {
            case "Keyboard&Mouse":
                ChangeScheme = InputScheme.KeyboardMouse;
                gamepadVibration.enabled = false;
                break;
            case "Gamepad":
                ChangeScheme = InputScheme.Gamepad;
                gamepadVibration.enabled = true;
                break;
        }
    }
}
