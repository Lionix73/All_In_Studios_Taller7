using System.Collections;
using UnityEngine;

public class MultiGlassCannon : MultiPassiveSkillBase
{
    [SerializeField][Range(1, 100)] private int increasedDamagePercentage = 100;
    [SerializeField][Range(10, 90)] private float remainingHealthPercentage = 50;

    private GunManagerMulti2 gunManager;
    private HealthMulti playerHealth;
    private GunType lastGun;
    private bool healthApplied = false;
    private bool damageApplied = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        gunManager = transform.root.GetComponentInChildren<GunManagerMulti2>();
        playerHealth = GetComponentInParent<HealthMulti>();

        lastGun = gunManager.CurrentGun.Type;
    }

    public override void CheckCondition()
    {
        if (gunManager.weapon == null) return;
        
        if(lastGun != gunManager.weapon.Type)
        {
            lastGun = gunManager.weapon.Type;
            damageApplied = false;
        }
        
        if(!damageApplied || !healthApplied) StartCoroutine(Execute());
    }

    public override IEnumerator Execute()
    {
        if(!healthApplied)
        {
            float reducedHealth = playerHealth.MaxHealth * (remainingHealthPercentage / 100);
            Debug.Log(reducedHealth);
            playerHealth.TakeDamage((int)reducedHealth, 10);
            healthApplied = true;
        }
        
        if(!damageApplied)
        {
            gunManager.weapon.Damage *= (1 + (increasedDamagePercentage / 100));
            damageApplied = true;
        }

        yield return null;
    }
}
