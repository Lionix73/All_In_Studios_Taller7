using System;
using System.Linq;
using UnityEngine;

public class PassiveSkillManager : SkillsManagerBase
{
    public PassiveSkillBase[] passiveSkills;
    public int activeSkillIndex;

    private void Start()
    {
        DeactivateUnusedSkills();

        SearchSkillsUI();
        _pasSkillImg.sprite = passiveSkills[activeSkillIndex].skillInfo.image;
        _pasSkillMask.sprite = passiveSkills[activeSkillIndex].skillInfo.image;
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
