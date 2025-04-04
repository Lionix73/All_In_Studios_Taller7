using System.Collections;
using UnityEngine;

public abstract class SkillBase : MonoBehaviour, ISkill
{
    protected float cooldown;
    protected bool isOnCooldown = false;

    public bool IsOnCooldown => isOnCooldown;

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
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }
}
