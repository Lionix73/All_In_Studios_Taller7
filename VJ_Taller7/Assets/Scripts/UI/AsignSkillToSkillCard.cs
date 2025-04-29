using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AsignSkillToSkillCard : MonoBehaviour
{
    [Header("Skill Info")]
    [Tooltip("Sctiptable object con la info de la skill")]
    public Skill_Info info;
    
    private Image icon;
    private TextMeshProUGUI[] texts;

    void Start()
    {
        icon = GetComponentInChildren<Image>();
        texts = GetComponentsInChildren<TextMeshProUGUI>();

        AsignInfo();
    }

    private void AsignInfo()
    {
        icon.sprite = info.image;
        texts[0].text = info.title;

        int lengthDescriptionList = info.descriptionItems.Length;

        for (int i = 0; i < lengthDescriptionList; i++)
        {
            texts[i+1].text = info.descriptionItems[i];
        }
        
        for (int i = 0; i < lengthDescriptionList; i++)
        {
            texts[i+5].text = info.descriptionText[i];
        }
    }
}
