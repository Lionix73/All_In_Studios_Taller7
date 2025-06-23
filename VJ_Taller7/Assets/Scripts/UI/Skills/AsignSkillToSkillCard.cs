using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AsignSkillToSkillCard : MonoBehaviour
{
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
    }

    public void AsignInfo(Skill_Info info)
    {
        iconSkill.sprite = info.image;
        bGSkill.sprite = info.backgroundImage;
        titleSkill.sprite = info.titleImage;
        //texts[0].text = info.title;

        // Asignar descriptionItems (índices 1-4)
        for (int i = 0; i < 4; i++)
        {
            texts[i].text = i < info.descriptionItems.Length ?
                info.descriptionItems[i] : string.Empty;
        }

        // Asignar descriptionText (índices 5-8)
        for (int i = 0; i < 4; i++)
        {
            texts[i + 4].text = i < info.descriptionText.Length ?
                info.descriptionText[i] : string.Empty;
        }
    }
}