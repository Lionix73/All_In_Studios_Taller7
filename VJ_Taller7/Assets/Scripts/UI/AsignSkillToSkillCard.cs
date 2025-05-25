using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AsignSkillToSkillCard : MonoBehaviour
{
    [Header("Skill Info")]
    [Tooltip("Sctiptable object con la info de la skill")]
    //public Skill_Info info;
    
    private Image iconSkill;
    private Image bGSkill;
    private Image titleSkill;
    private TextMeshProUGUI[] texts;

    void Awake()
    {
        iconSkill = GameObject.Find("IconSkill").GetComponent<Image>();
        bGSkill = GameObject.Find("BGSkill").GetComponent<Image>();
        titleSkill = GameObject.Find("TitleSkill").GetComponent<Image>();
        texts = GetComponentsInChildren<TextMeshProUGUI>();

        //AsignInfo();
    }

    public void AsignInfo(Skill_Info info)
    {
        iconSkill.sprite = info.image;
        bGSkill.sprite = info.backgroundImage;
        titleSkill.sprite = info.titleImage;
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
