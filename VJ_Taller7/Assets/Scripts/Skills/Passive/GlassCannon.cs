using System.Collections;
using UnityEngine;

public class GlassCannon : PassiveSkillBase
{
    [SerializeField][Range(1, 100)] private int increasedDamagePercentage = 100;
    [SerializeField][Range(10, 90)] private int remainingHealthPercentage = 50;

    private GunManager gunManager;
    private Health playerHealth;
    private GunType lastGun;
    private bool healthApplied = false;
    private bool damageApplied = false;

    private void Start()
    {
        gunManager = FindAnyObjectByType<GunManager>();
        playerHealth = GetComponentInParent<Health>();

        lastGun = gunManager.CurrentGun.Type;
    }

    public override void CheckCondition()
    {
        if(lastGun != gunManager.CurrentGun.Type)
        {
            lastGun = gunManager.CurrentGun.Type;
            damageApplied = false;
        }
        
        if(!damageApplied || !healthApplied) StartCoroutine(Execute());
    }

    public override IEnumerator Execute()
    {
        if(!healthApplied)
        {
            float reducedHealth = playerHealth.GetMaxHeath * ((float)remainingHealthPercentage / 100);
            playerHealth.ScaleHealth(-reducedHealth);
            healthApplied = true;
        }
        
        if(!damageApplied)
        {
            gunManager.CurrentGun.Damage *= (1 + (increasedDamagePercentage / 100));
            damageApplied = true;
        }

        yield return null;
    }
}
