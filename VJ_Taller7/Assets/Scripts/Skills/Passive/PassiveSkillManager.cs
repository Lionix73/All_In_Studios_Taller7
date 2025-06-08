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

        DeactivateUnusedSkills();
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
        if (!passiveSkills[activeSkillIndex].IsOnCooldown)
            passiveSkills[activeSkillIndex].Activate();
    }

    private void DeactivateUnusedSkills()
    {
        for (int i = 0; i < passiveSkills.Length; i++)
        {
            if (i != activeSkillIndex)
            {
                passiveSkills[i].gameObject.SetActive(false);
            }
        }
    }
}
