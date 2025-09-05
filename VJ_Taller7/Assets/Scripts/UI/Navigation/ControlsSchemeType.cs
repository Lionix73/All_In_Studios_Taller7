using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ControlsSchemeType : MonoBehaviour
{
    [SerializeField] private List<ControlSchemes> controlsSchemes;
    [SerializeField] private TextMeshProUGUI titleText;
    private int index;

    public void ChangeScheme()
    {
        controlsSchemes[index].content.SetActive(false);

        if (index < controlsSchemes.Count - 1)
            index++;
        else
            index = 0;

        controlsSchemes[index].content.SetActive(true);
        titleText.text = controlsSchemes[index].title;
    }
}

[System.Serializable]
    public struct ControlSchemes
    {
        public string title;
        public GameObject content;
    }