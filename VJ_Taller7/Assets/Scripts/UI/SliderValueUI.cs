using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

[DefaultExecutionOrder(-1)]
public class SliderValueUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI valueText;
    private Slider slider;

    void Start()
    {
        slider = GetComponentInChildren<Slider>();
        UpdateSlideNumber();
    }

    public void UpdateSlideNumber()
    {
        //valueText.text = Mathf.FloorToInt(slider.value).ToString();
        valueText.text = Math.Round(slider.value, 1).ToString();
    }
}
