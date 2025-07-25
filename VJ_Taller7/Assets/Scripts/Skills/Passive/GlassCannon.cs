using System.Collections;
using UnityEngine;

public class GlassCannon : PassiveSkillBase
{
    [SerializeField][Range(1, 100)] private int increasedDamagePercentage = 100;
    [SerializeField][Range(10, 90)] private int remainingHealthPercentage = 50;

    private GunManager gunManager;
    private Health playerHealth;
    private GunType lastGun = GunType.BasicPistol;
    private bool healthApplied = false;
    private bool damageApplied = false;

    private void Awake()
    {
        gunManager = transform.root.GetComponentInChildren<GunManager>();
        playerHealth = GetComponentInParent<Health>();
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

            UIManager.Singleton.GetPlayerTotalHealth(playerHealth.GetMaxHeath);
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
