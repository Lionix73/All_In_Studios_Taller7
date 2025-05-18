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
    public Button passiveButton;
    public Button activeButton;
    public Button upArrowButton;
    public Button downArrowButton;
    public TextMeshProUGUI skillTypeText;

    private List<Skill_Info> currentSkills;
    private int selectedIndex = 0;
    private bool viewingPassiveSkills = true;

    private void Start()
    {
        // Configurar listeners de botones
        passiveButton.onClick.AddListener(() => SwitchToPassiveSkills());
        activeButton.onClick.AddListener(() => SwitchToActiveSkills());
        upArrowButton.onClick.AddListener(() => NavigateSkills(-1));
        downArrowButton.onClick.AddListener(() => NavigateSkills(1));
        SelectedActiveSkill(selectedIndex);
        SelectedPassiveSkill(selectedIndex);
        // Inicializar mostrando habilidades pasivas
        SwitchToPassiveSkills();
    }

    private void SwitchToPassiveSkills()
    {
        viewingPassiveSkills = true;
        currentSkills = passiveSkills;
        selectedIndex = Mathf.Clamp(CharacterManager.Instance.indexPassiveSkill, 0, currentSkills.Count - 1);
        UpdateSkillDisplay();
        //skillTypeText.text = "Habilidades Pasivas";
    }

    private void SwitchToActiveSkills()
    {
        viewingPassiveSkills = false;
        currentSkills = activeSkills;
        selectedIndex = Mathf.Clamp(CharacterManager.Instance.indexActiveSkill, 0, currentSkills.Count - 1);
        UpdateSkillDisplay();
        //skillTypeText.text = "Habilidades Activas";
    }

    private void NavigateSkills(int direction)
    {
        if (currentSkills == null || currentSkills.Count == 0) return;

        selectedIndex += direction;

        // Asegurarse de que el índice esté dentro de los límites
        if (selectedIndex < 0) selectedIndex = currentSkills.Count - 1;
        else if (selectedIndex >= currentSkills.Count) selectedIndex = 0;

        UpdateSkillDisplay();

        // Actualizar el índice en el CharacterManager
        if (viewingPassiveSkills)
        {

            CharacterManager.Instance.indexPassiveSkill = selectedIndex;
            SelectedPassiveSkill(selectedIndex);
        }
        else
        {
            CharacterManager.Instance.indexActiveSkill = selectedIndex;
            SelectedActiveSkill(selectedIndex);
        }
    }

    private void UpdateSkillDisplay()
    {
        if (currentSkills != null && currentSkills.Count > 0 && selectedIndex >= 0 && selectedIndex < currentSkills.Count)
        {
            cardSkillInfo.AsignInfo(currentSkills[selectedIndex]);
        }
    }

    public void SelectedPassiveSkill(int selectedIndex)
    {
        CharacterManager.Instance.indexPassiveSkill = selectedIndex;
        Skill_Info skillInfo = passiveSkills[selectedIndex];
        cardSkillInfo.AsignInfo(skillInfo);
        Image passiveSkillImage = passiveButton.GetComponent<Image>();
        passiveSkillImage.sprite = skillInfo.image;
    }

    public void SelectedActiveSkill(int selectedIndex)
    {
        CharacterManager.Instance.indexActiveSkill = selectedIndex;
        Skill_Info skillInfo = activeSkills[selectedIndex];
        Image activeSkillImage = activeButton.GetComponent<Image>();
        activeSkillImage.sprite = skillInfo.image;
    }
}