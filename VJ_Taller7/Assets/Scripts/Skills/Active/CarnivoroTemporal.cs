using System.Collections;
using FMODUnity;
using UnityEngine;

public class CarnivoroTemporal : SkillBase
{
    [SerializeField] private float duration = 10f;
    [SerializeField] private float cooldownThisSkill = 20f;
    [SerializeField][Range(1, 100)][Tooltip("health % of the max health the player recover")] private int healthPerecentageToRecover = 10;

    private Health playerHealth;
    private bool carnivoroTemp;

    public bool CarnivoroActive
    {
        get => carnivoroTemp;
        set => carnivoroTemp = value;
    }

    private void Start()
    {
        playerHealth = GetComponentInParent<Health>();
        cooldown = cooldownThisSkill;
    }

    public override IEnumerator Execute()
    {
        CarnivoroActive = true;
        RuntimeManager.PlayOneShotAttached(skillInfo.activateSkillSound, gameObject);

        yield return new WaitForSeconds(duration);

        CarnivoroActive = false;
    }

    public void HealthForKill()
    {
        float healthAmountToRecover = playerHealth.GetMaxHeath * ((float)healthPerecentageToRecover / 100);
        playerHealth.TakeHeal(healthAmountToRecover);
    }
}
