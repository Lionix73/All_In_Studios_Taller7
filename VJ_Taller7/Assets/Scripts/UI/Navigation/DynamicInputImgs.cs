using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DynamicInputImgs : MonoBehaviour
{
    [Header("Icons")]
    [SerializeField] private Sprite keyboardSprite;
    [SerializeField] private Sprite xboxSprite;
    [SerializeField] private Sprite psSprite;

    private Image image;
    private ControlsSchemeManager controls;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        Invoke(nameof(FindControls), 0.5f);
    }

    private void FindControls()
    {
        controls = ControlsSchemeManager.Instance;
        controls.OnControlsChange += UpdateIcon;
        UpdateIcon();
    }

    private void OnDestroy()
    {
        controls.OnControlsChange -= UpdateIcon;
    }

    private void UpdateIcon()
    {
        switch (controls.ChangeScheme)
        {
            case ControlsSchemeManager.InputScheme.XboxGamepad:
                image.sprite = xboxSprite;
                break;
            case ControlsSchemeManager.InputScheme.PSGamepad:
                image.sprite = psSprite;
                break;
            case ControlsSchemeManager.InputScheme.KeyboardMouse:
                image.sprite = keyboardSprite;
                break;
        }
    }
}