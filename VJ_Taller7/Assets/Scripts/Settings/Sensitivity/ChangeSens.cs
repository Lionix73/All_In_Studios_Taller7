using UnityEngine;
using UnityEngine.UI;

public class ChangeSens : MonoBehaviour
{
    [SerializeField] private SettingsSO _sensiManager;
    [SerializeField] private bool vertical;
    
    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void Start()
    {
        if (vertical)
        {
            _slider.value = Mathf.Abs(_sensiManager.SensibilityGainY);
        }
        else
            _slider.value = _sensiManager.SensibilityGainX;
    }

    public void ChangeSensGain()
    {
        if (!vertical)
        {
            if (_sensiManager != null)
            {
                float newValue = _slider.value;

                _sensiManager.SensibilityGainX = newValue;
            }
        }
        else
        {
            if (_sensiManager != null)
            {
                float newValue = _slider.value * -1;

                _sensiManager.SensibilityGainY = newValue;
            }
        }
    }
}
