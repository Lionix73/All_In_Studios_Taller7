using UnityEngine;
using UnityEngine.InputSystem;

public class EmotesMenuController : MonoBehaviour
{
    private RingMenu ringMenu;

    private void Start()
    {
        ringMenu = FindFirstObjectByType<RingMenu>();
        ringMenu.gameObject.SetActive(false);
    }

    public void ActivateMenu(InputAction.CallbackContext callbackContext)
    {   
        if (callbackContext.started)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            ringMenu.gameObject.SetActive(true);
        }
            
        if (callbackContext.canceled)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            ringMenu.gameObject.SetActive(false);
            ringMenu.Emotear();
        }
    }
}
