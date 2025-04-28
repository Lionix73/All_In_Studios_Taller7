using System.Collections;
using UnityEngine;

public abstract class SkillBase : MonoBehaviour, ISkill
{
    private ActiveSkillManager skillManager;
    public Skill_Info skillInfo;

    protected float cooldown;
    protected bool isOnCooldown = false;

    public float WhatIsTheCooldown => cooldown;
    public bool IsOnCooldown => isOnCooldown;

    private void Awake()
    {
        skillManager = GetComponentInParent<ActiveSkillManager>();
    }

    public virtual void Activate()
    {
        if (!isOnCooldown)
        {
            StartCoroutine(skillManager.DecreaseActiveSkillMask(skillManager.skills[skillManager.activeSkillIndex].WhatIsTheCooldown));
            StartCoroutine(Execute());
            StartCoroutine(CooldownRoutine());
        }
    }

    public abstract IEnumerator Execute();

    protected virtual IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }
}
