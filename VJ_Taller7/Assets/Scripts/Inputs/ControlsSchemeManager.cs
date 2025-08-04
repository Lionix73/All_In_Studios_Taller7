using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsSchemeManager : Singleton<ControlsSchemeManager>
{
    #region Controls Scheme
    public enum InputScheme
    {
        KeyboardMouse,
        XboxGamepad,
        PSGamepad
    }

    [SerializeField] private InputScheme currentScheme = InputScheme.KeyboardMouse;
    public InputScheme ChangeScheme
    {
        get => currentScheme;
        private set => currentScheme = value;
    }
    #endregion

    private PlayerInput playerInput;
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
                // Detect gamepad type
                bool isXbox = false;
                bool isPS = false;
                foreach (var device in scheme.devices)
                {
                    if (device is Gamepad)
                    {
                        var prod = device.description.product?.ToLower() ?? "";
                        var name = device.displayName?.ToLower() ?? "";
                        if (prod.Contains("xbox") || name.Contains("xbox"))
                            isXbox = true;
                        else if (prod.Contains("dualshock") || prod.Contains("playstation") || name.Contains("ps"))
                            isPS = true;
                    }
                }
                if (isXbox)
                {
                    ChangeScheme = InputScheme.XboxGamepad;
                }
                else if (isPS)
                {
                    ChangeScheme = InputScheme.PSGamepad;
                }
                else
                {
                    ChangeScheme = InputScheme.XboxGamepad; // Default to Xbox if unknown
                }
                gamepadVibration.enabled = true;
                break;
        }
    }
}
