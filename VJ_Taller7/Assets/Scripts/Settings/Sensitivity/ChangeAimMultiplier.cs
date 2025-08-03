using UnityEngine;
using UnityEngine.UI;

public class ChangeAimMultiplier : MonoBehaviour
{
    [SerializeField] private SettingsSO _sensiManager;
    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void Start()
    {
        _slider.value = _sensiManager.AimSensiMultiplier;
    }

    public void ChangeAimSensitivity()
    {
        if (_sensiManager != null)
        {
            float newValue = _slider.value;

            _sensiManager.AimSensiMultiplier = newValue;
        }
    }
}
