using System.Collections;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using UnityEngine;

public class SoyTuAmadaCarga : PassiveSkillBase
{
    private GunManager gunManager;
    private Health playerHealth;

    private void Start()
    {
        gunManager = FindAnyObjectByType<GunManager>();
        playerHealth = GetComponentInParent<Health>();
    }

    public override void CheckCondition()
    {
        
    }

    public override IEnumerator Execute()
    {
        
        yield return null;
    }
}
