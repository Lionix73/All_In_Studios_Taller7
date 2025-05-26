using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillMenu : MonoBehaviour
{
    [Header("Skill Lists")]
    public List<Skill_Info> allPassiveSkills; // Todas las habilidades pasivas
    public List<Skill_Info> allActiveSkills;  // Todas las habilidades activas

    [Header("UI References")]
    public AsignSkillToSkillCard cardSkillInfo;
    public Button passiveButton;
    public Button activeButton;
    public Button upArrowButton;
    public Button downArrowButton;
    public TextMeshProUGUI skillTypeText;

    private List<Skill_Info> currentSkills;    // Habilidades actualmente mostradas
    private List<Skill_Info> passiveSkills;    // Habilidades pasivas filtradas
    private List<Skill_Info> activeSkills;     // Habilidades activas filtradas
    private int selectedIndex = 0;
    private bool viewingPassiveSkills = true;
    private bool isMultiplayerMode = false;    // Cambiar según el modo de juego

    private void Start()
    {
        // Inicializar listas filtradas según el modo
        FilterSkillsByGameMode();

        // Configurar listeners de botones
        passiveButton.onClick.AddListener(() => SwitchToPassiveSkills());
        activeButton.onClick.AddListener(() => SwitchToActiveSkills());
        upArrowButton.onClick.AddListener(() => NavigateSkills(-1));
        downArrowButton.onClick.AddListener(() => NavigateSkills(1));

        // Inicializar mostrando habilidades pasivas
        SwitchToPassiveSkills();
    }

    // Filtra habilidades según el modo de juego
    private void FilterSkillsByGameMode()
    {
        passiveSkills = new List<Skill_Info>();
        activeSkills = new List<Skill_Info>();

        foreach (var skill in allPassiveSkills)
        {
            // Si es Singleplayer, solo mostrar habilidades !isMultiplayer
            // Si es Multiplayer, mostrar todas o solo isMultiplayer (según necesidad)
            if (!isMultiplayerMode && !skill.isMultiplayer || isMultiplayerMode)
            {
                passiveSkills.Add(skill);
            }
        }

        foreach (var skill in allActiveSkills)
        {
            if (!isMultiplayerMode && !skill.isMultiplayer || isMultiplayerMode)
            {
                activeSkills.Add(skill);
            }
        }
    }

    // Cambiar entre modos (Singleplayer/Multiplayer)
    public void SetGameMode(bool multiplayer)
    {
        isMultiplayerMode = multiplayer;
        FilterSkillsByGameMode();

        // Resetear la visualización
        if (viewingPassiveSkills)
            SwitchToPassiveSkills();
        else
            SwitchToActiveSkills();

        SelectedActiveSkill(selectedIndex);
        SelectedPassiveSkill(selectedIndex);
    }

    private void SwitchToPassiveSkills()
    {
        viewingPassiveSkills = true;
        currentSkills = passiveSkills;
        selectedIndex = Mathf.Clamp(CharacterManager.Instance.indexPassiveSkill, 0, currentSkills.Count - 1);
        UpdateSkillDisplay();
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