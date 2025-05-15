using UnityEngine;
using TMPro;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class SliderValueUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI valueText;
    private Slider slider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slider = GetComponentInChildren<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateSlideNumber()
    {
        valueText.text = Mathf.FloorToInt(slider.value).ToString();
    }
}
