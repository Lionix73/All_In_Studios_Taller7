using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DetectDevice;

public class DynamicInputIcon : MonoBehaviour
{
    [Header("Íconos")]
    public string keyboardSprite;
    public Sprite xboxSprite;
    public Sprite playStationSprite;

    private Image image;
    private TextMeshProUGUI text;

    private void Awake()
    {
        image = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        UpdateIcon(InputDeviceManager.CurrentScheme);
        InputDeviceManager.OnInputSchemeChanged += UpdateIcon;
    }

    private void OnDestroy()
    {
        InputDeviceManager.OnInputSchemeChanged -= UpdateIcon;
    }

    private void UpdateIcon(InputScheme scheme)
    {
        switch (scheme)
        {
            case InputScheme.XboxGamepad:
                image.color = new Color(255, 255, 255, 255);
                image.sprite = xboxSprite;
                text.text = "";
                break;
            case InputScheme.PlayStationGamepad:
                image.color = new Color(255, 255, 255, 255);
                image.sprite = playStationSprite;
                text.text = "";
                break;
            case InputScheme.KeyboardMouse:
                text.text = keyboardSprite;
                image.color = new Color(255, 255, 255, 0);
                break;
            default:
                text.text = keyboardSprite;
                break;
        }
    }
}
