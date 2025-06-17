using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

[DefaultExecutionOrder(-1)]
public class SliderValueUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI valueText;
    [Space]
    [SerializeField][Range(0, 3)] private int numberOfTenths = 1;
    [SerializeField] private float multiplier = 1f;
    private Slider slider;

    void Start()
    {
        slider = GetComponentInChildren<Slider>();
        UpdateSlideNumber();
    }

    public void UpdateSlideNumber()
    {
        valueText.text = Math.Round(slider.value * multiplier, numberOfTenths).ToString();
    }
}
