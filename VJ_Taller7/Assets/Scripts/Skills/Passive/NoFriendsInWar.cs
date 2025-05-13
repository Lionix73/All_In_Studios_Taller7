using System.Collections;
using UnityEngine;

public class NoFriendsInWar : PassiveSkillBase
{
    [SerializeField][Range(1, 100)] private int increasedDamagePercentage = 15;

    private GunManager _gunManager;
    private GunType _lastGun;
    private bool _damageApplied = false;

    private void Awake()
    {
        _gunManager = transform.root.GetComponentInChildren<GunManager>();

        _lastGun = _gunManager.CurrentGun.Type;
    }

    public override void CheckCondition()
    {
        if(_lastGun != _gunManager.CurrentGun.Type)
        {
            _lastGun = _gunManager.CurrentGun.Type;
            _damageApplied = false;
        }
        
        if(!_damageApplied) StartCoroutine(Execute());
    }

    public override IEnumerator Execute()
    {   
        if(!_damageApplied)
        {
            _gunManager.CurrentGun.Damage *= (1 + (increasedDamagePercentage / 100));
            _damageApplied = true;

            _gunManager.CurrentGun.noFriendsInWar = true;
        }

        yield return null;
    }

    private void OnDisable()
    {
        _gunManager.CurrentGun.noFriendsInWar = false;
    }
}
