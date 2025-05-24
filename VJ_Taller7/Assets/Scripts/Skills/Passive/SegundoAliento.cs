using System.Collections;
using UnityEngine;

public class SegundoAliento : PassiveSkillBase
{
    [SerializeField] private float cooldownThisSkill = 90f;
    
    private Health playerHealth;
    private bool secondBreathActive = false;

    public bool IsSecondBreathActive
    {
        get => secondBreathActive;
        set => secondBreathActive = value;
    }

    private void Start()
    {
        playerHealth = GetComponentInParent<Health>();
        cooldown = cooldownThisSkill;
    }

    public override void CheckCondition()
    {
        IsSecondBreathActive = true;

        if (playerHealth.GetCurrentHeath <= playerHealth.GetMaxHeath * 0.1f && secondBreathActive)
        {
            IsSecondBreathActive = false;
            StartCoroutine(Execute());
            StartCoroutine(CooldownRoutine());
        }
    }

    public override IEnumerator Execute()
    {
        soundManager.PlaySound("SecondBreath");

        playerHealth.TakeHeal(playerHealth.GetMaxHeath);
        yield return null;
    }
}
