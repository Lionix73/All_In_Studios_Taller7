using System.Collections;
using UnityEngine;

public abstract class SkillBase : MonoBehaviour, ISkill
{
    [ExposedScriptableObject] public Skill_Info skillInfo;
    private ActiveSkillManager skillManager;
    protected ThisObjectSounds soundManager;

    protected float cooldown;
    protected bool isOnCooldown = false;

    public float WhatIsTheCooldown => cooldown;
    public bool IsOnCooldown => isOnCooldown;

    private void Awake()
    {
        skillManager = GetComponentInParent<ActiveSkillManager>();
        soundManager = GetComponentInParent<ThisObjectSounds>();
    }

    public virtual void Activate()
    {
        if (!isOnCooldown)
        {
            StartCoroutine(Execute());
            StartCoroutine(CooldownRoutine());
        }
    }

    public abstract IEnumerator Execute();

    protected virtual IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        skillManager.ActivateMask(true, cooldown);
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
        soundManager.PlaySound("ReloadSkill");
    }
}
