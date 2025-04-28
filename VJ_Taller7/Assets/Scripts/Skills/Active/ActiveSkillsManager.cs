using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ActiveSkillManager : SkillsManagerBase
{
    public SkillBase[] skills;
    public int activeSkillIndex;

    private void Start()
    {
        DeactivateUnusedSkills();

        SearchSkillsUI();
        _actSkillImg.sprite = skills[activeSkillIndex].skillInfo.image;
        _actSkillMask.sprite = skills[activeSkillIndex].skillInfo.image;
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
