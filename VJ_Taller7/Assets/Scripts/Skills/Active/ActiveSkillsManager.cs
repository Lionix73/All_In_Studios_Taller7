using UnityEngine;

public class ActiveSkillManager : SkillsManagerBase
{
    public SkillBase[] skills;
    public int activeSkillIndex;

    private void Start()
    {
        if (CharacterManager.Instance != null)
            activeSkillIndex = CharacterManager.Instance.indexActiveSkill;

        DeactivateUnusedSkills();
        SearchSkillsUI();
        SetupSkillsIcons();
    }

    private void SetupSkillsIcons()
    {
        Sprite skillSprite = skills[activeSkillIndex].skillInfo.image;
        SetupShaderMaterial(_actSkillImg, skillSprite);
    }

    private void DeactivateUnusedSkills()
    {
        for (int i = 0; i < skills.Length; i++)
        {
            if (i != activeSkillIndex)
            {
                skills[i].gameObject.SetActive(false);
            }
        }
    }

    public void ActivateSkill()
    {
        if (activeSkillIndex >= 0 && activeSkillIndex < skills.Length)
        {
            skills[activeSkillIndex].Activate();
        }
        else
        {
            Debug.LogWarning("Índice de habilidad fuera de rango.");
        }
    }
}
