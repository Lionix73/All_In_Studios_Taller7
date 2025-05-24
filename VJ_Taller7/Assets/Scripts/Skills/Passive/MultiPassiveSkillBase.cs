using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class MultiPassiveSkillBase : NetworkBehaviour, IPassiveSkill
{
    private MultiPassiveSkillManager skillManager;
    public Skill_Info skillInfo;

    protected float cooldown;
    protected bool isOnCooldown = false;

    public float WhatIsTheCooldown => cooldown;
    public bool IsOnCooldown => isOnCooldown;

    private void Awake()
    {
        skillManager = GetComponentInParent<MultiPassiveSkillManager>();
    }

    public virtual void Activate()
    {
        if (!isOnCooldown)
        {
            skillManager.DecreasePassiveSkillMaskRpc(skillManager.passiveSkills[skillManager.activeSkillIndex].WhatIsTheCooldown);
            CheckCondition();
        }
    }

    public abstract void CheckCondition(); // Cada skill define cuándo se activa

    public abstract IEnumerator Execute(); // Cada skill implementa su efecto

    protected IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }
}
