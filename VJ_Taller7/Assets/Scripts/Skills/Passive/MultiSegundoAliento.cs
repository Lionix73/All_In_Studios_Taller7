using System.Collections;
using UnityEngine;

public class MultiSegundoAliento : MultiPassiveSkillBase
{
    [SerializeField] private float cooldownThisSkill = 90f;
    private HealthMulti playerHealth;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerHealth = GetComponentInParent<HealthMulti>();
    }

    public override void CheckCondition()
    {
        if (playerHealth.CurrentHealth <= playerHealth.MaxHealth * 0.1f)
        {
            cooldown = cooldownThisSkill;
            StartCoroutine(Execute());
            StartCoroutine(CooldownRoutine());
        }
    }

    public override IEnumerator Execute()
    {
        playerHealth.TakeHeal((int)playerHealth.MaxHealth);
        yield return null;
    }
}
