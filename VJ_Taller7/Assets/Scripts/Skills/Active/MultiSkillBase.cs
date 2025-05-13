using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class MultiSkillBase : NetworkBehaviour, ISkill
{
    private MultiActiveSkillManager skillManager;
    public Skill_Info skillInfo;

    protected float cooldown;
    protected bool isOnCooldown = false;

    public float WhatIsTheCooldown => cooldown;
    public bool IsOnCooldown => isOnCooldown;

    private void Awake()
    {
        skillManager = GetComponentInParent<MultiActiveSkillManager>();
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
