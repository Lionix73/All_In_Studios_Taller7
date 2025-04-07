using System.Collections;
using UnityEngine;

public abstract class PassiveSkillBase : MonoBehaviour, IPassiveSkill
{
    protected float cooldown;
    protected bool isOnCooldown = false;

    public bool IsOnCooldown => isOnCooldown;

    public abstract void CheckCondition(); // Cada skill define cuándo se activa

    public abstract IEnumerator Execute(); // Cada skill implementa su efecto

    protected IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }
}
