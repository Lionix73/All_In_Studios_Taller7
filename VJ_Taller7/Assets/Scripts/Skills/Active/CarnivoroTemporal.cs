using System.Collections;
using UnityEngine;

public class CarnivoroTemporal : SkillBase
{
    [SerializeField] private float duration = 10f;
    [SerializeField] private float cooldownThisSkill = 20f;
    [SerializeField][Range(1, 100)][Tooltip("health % of the max health the player recover")] private int healthPerecentageToRecover = 10;
    [SerializeField] private bool carnivoroTemp;

    private Health playerHealth;

    public bool Carnivoro
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
        Carnivoro = true;

        yield return new WaitForSeconds(duration);

        Carnivoro = false;
    }

    private void HealthForKill()
    {
        float healthAmountToRecover = playerHealth.GetMaxHeath * (healthPerecentageToRecover / 100);

        playerHealth.TakeHeal(healthAmountToRecover);
    }
}
