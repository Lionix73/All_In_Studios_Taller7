using System.Collections;
using UnityEngine;

public class MultiCarnivoroTemporal : MultiSkillBase
{
    [SerializeField] private float duration = 10f;
    [SerializeField] private float cooldownThisSkill = 20f;
    [SerializeField][Range(1, 100)][Tooltip("health % of the max health the player recover")] private int healthPerecentageToRecover = 10;
    [SerializeField] private bool carnivoroTemp;

    private HealthMulti playerHealth;

    public bool Carnivoro
    {
        get => carnivoroTemp;
        set => carnivoroTemp = value;
    }

    private void Start()
    {
        playerHealth = GetComponentInParent<HealthMulti>();
        cooldown = cooldownThisSkill;
    }

    public override IEnumerator Execute()
    {
        Carnivoro = true;

        yield return new WaitForSeconds(duration);

        Carnivoro = false;
    }

    public void HealthForKill()
    {
        float healthAmountToRecover = playerHealth.MaxHealth * (healthPerecentageToRecover / 100);

        playerHealth.TakeHeal((int)healthAmountToRecover);
    }
}
