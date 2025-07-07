using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DynamicInputIcon : MonoBehaviour
{
    [Header("Icons")]
    public string keyboardSprite;
    public Sprite xboxSprite;

    private Image image;
    private TextMeshProUGUI text;
    private ControlsSchemeManager controls;

    private void Awake()
    {
        image = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
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
            case ControlsSchemeManager.InputScheme.Gamepad:
                image.color = new Color(255, 255, 255, 255);
                image.sprite = xboxSprite;
                text.text = "";
                break;
            case ControlsSchemeManager.InputScheme.KeyboardMouse:
                text.text = keyboardSprite;
                image.color = new Color(255, 255, 255, 0);
                break;
        }
    }
}