using System;
using System.Linq;
using UnityEngine;

public class MultiPassiveSkillManager : MultiSkillsManagerBase
{
    public MultiPassiveSkillBase[] passiveSkills;
    public int activeSkillIndex;

    private void Start()
    {
        if (CharacterManager.Instance != null)
            activeSkillIndex = CharacterManager.Instance.indexPassiveSkill;

        DeactivateUnusedSkills();

        SearchSkillsUI();
        _pasSkillImg.sprite = passiveSkills[activeSkillIndex].skillInfo.image;
        _pasSkillMask.sprite = passiveSkills[activeSkillIndex].skillInfo.image;
    }

    private void Update()
    {
        if(!IsServer && NetworkObject.IsSpawned) return;
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
