using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillMenu : MonoBehaviour
{
    [Header("Skill Lists")]
    public List<Skill_Info> passiveSkills;
    public List<Skill_Info> activeSkills;

    [Header("UI References")]
    public AsignSkillToSkillCard cardSkillInfo;
    public Transform passiveSkillsContainer;
    public Transform activeSkillsContainer;
    public GameObject skillButtonPrefab;

    int selectedIndex = 0;

    private void Start() 
    { 
        
        cardSkillInfo.AsignInfo(passiveSkills[selectedIndex]);
        // Crear botones para habilidades pasivas
        CreateSkillButtons(passiveSkills, passiveSkillsContainer, true);

        // Crear botones para habilidades activas
        CreateSkillButtons(activeSkills, activeSkillsContainer, false);
    }

    private void CreateSkillButtons(List<Skill_Info> skills, Transform container, bool isPassive)
    {
        // Limpiar contenedor si ya tiene botones
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        // Crear un botón por cada habilidad
        for (int i = 0; i < skills.Count; i++)
        {
            int index = i; // Importante: crear copia local para el closure
            var buttonObj = Instantiate(skillButtonPrefab, container);
            var button = buttonObj.GetComponent<Button>();

            // Configurar imagen y texto del botón
            var icon = buttonObj.GetComponent<Image>();
            //var text = buttonObj.GetComponentInChildren<TMP_Text>();

            if (icon != null) icon.sprite = skills[i].image;
            //if (text != null) text.text = skills[i].title;

            // Asignar evento click
            button.onClick.AddListener(() => {
                if (isPassive)
                {
                    SelectedPassiveSkill(index);
                }
                else
                {
                    SelectedActiveSkill(index);
                }
            });
        }
    }
    public void SelectedPassiveSkill(int selectedIndex)
    {
        CharacterManager.Instance.indexPassiveSkill = selectedIndex;
        Skill_Info skillInfo = passiveSkills[selectedIndex];
        cardSkillInfo.AsignInfo(skillInfo);
    }

    public void SelectedActiveSkill(int selectedIndex)
    {
        CharacterManager.Instance.indexActiveSkill = selectedIndex;
        Skill_Info skillInfo = activeSkills[selectedIndex];
        cardSkillInfo.AsignInfo(skillInfo);
    }
}
