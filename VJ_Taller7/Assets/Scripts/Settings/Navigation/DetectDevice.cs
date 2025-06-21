using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class DetectDevice : MonoBehaviour
{
    public enum InputScheme { KeyboardMouse, XboxGamepad, PlayStationGamepad }

    void Awake()
    {
        InputDeviceManager.Initialize();
    }

    public static class InputDeviceManager
    {
        public static InputScheme CurrentScheme { get; private set; } = InputScheme.KeyboardMouse;

        public static event System.Action<InputScheme> OnInputSchemeChanged;

        public static void Initialize()
        {
            InputSystem.onAnyButtonPress.CallOnce(OnInput);
        }

        private static void OnInput(InputControl control)
        {
            var device = control.device;

            InputScheme newScheme = InputScheme.KeyboardMouse;
            //Debug.Log(device.displayName);

            if (device is Gamepad)
            {
                if (device.displayName == "Xbox Controller")
                    newScheme = InputScheme.XboxGamepad;
                else if (device.displayName == "PlayStation Controller")
                    newScheme = InputScheme.PlayStationGamepad;
                else
                    newScheme = InputScheme.XboxGamepad;
            }

            if (device is Mouse || device is Keyboard)
                newScheme = InputScheme.KeyboardMouse;

            if (newScheme != CurrentScheme)
            {
                CurrentScheme = newScheme;
                OnInputSchemeChanged?.Invoke(CurrentScheme);
            }

            InputSystem.onAnyButtonPress.CallOnce(OnInput);
        }
    }

}
