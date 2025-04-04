using System.Collections;
using UnityEngine;

public class SegundoAliento : PassiveSkillBase
{
    [SerializeField] private float cooldownThisSkill = 90f;
    private Health playerHealth;

    private void Start()
    {
        playerHealth = GetComponentInParent<Health>();
        cooldown = cooldownThisSkill;
    }

    public override void CheckCondition()
    {
        if (playerHealth.GetCurrentHeath <= playerHealth.GetMaxHeath * 0.1f && !IsOnCooldown)
        {
            StartCoroutine(Execute());
            StartCoroutine(CooldownRoutine());
        }
    }

    public override IEnumerator Execute()
    {
        playerHealth.TakeHeal(playerHealth.GetMaxHeath);
        yield return null;
    }
}
