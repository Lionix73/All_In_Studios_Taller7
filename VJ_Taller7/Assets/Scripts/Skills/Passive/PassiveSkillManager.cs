using UnityEngine;

public class PassiveSkillManager : SkillsManagerBase
{
    public PassiveSkillBase[] passiveSkills;
    public int activeSkillIndex;

    private void OnValidate()
    {
        activeSkillIndex = Mathf.Clamp(activeSkillIndex, 0, passiveSkills.Length);
    }

    private void Start()
    {
        if (CharacterManager.Instance != null)
            activeSkillIndex = CharacterManager.Instance.indexPassiveSkill;

        ActivateSkillGameObject();
        SearchSkillsUI();
        SetupSkillsIcons();
    }

    private void SetupSkillsIcons()
    {
        Sprite skillSprite = passiveSkills[activeSkillIndex].skillInfo.image;
        SetupShaderMaterial(_pasSkillImg, skillSprite);
    }

    private void Update()
    {
        passiveSkills[activeSkillIndex].Activate();
    }

    private void ActivateSkillGameObject()
    {
        passiveSkills[activeSkillIndex].gameObject.SetActive(true);
    }
}
