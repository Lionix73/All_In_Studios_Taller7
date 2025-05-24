using System.Collections;
using UnityEngine;

public class MultiNoFriendsInWar : MultiPassiveSkillBase
{
    [SerializeField][Range(1, 100)] private int increasedDamagePercentage = 15;

    private GunManagerMulti2 _gunManager;
    private GunType _lastGun;
    private bool _damageApplied = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _gunManager = transform.root.GetComponentInChildren<GunManagerMulti2>();

        _lastGun = _gunManager.weapon.Type;
    }

    public override void CheckCondition()
    {
        if(_lastGun != _gunManager.weapon.Type)
        {
            _lastGun = _gunManager.weapon.Type;
            _damageApplied = false;
        }
        
        if(!_damageApplied) StartCoroutine(Execute());
    }

    public override IEnumerator Execute()
    {   
        if(!_damageApplied)
        {
            _gunManager.weapon.Damage *= (1 + (increasedDamagePercentage / 100));
            _damageApplied = true;

            _gunManager.weapon.noFriendsInWar = true;
        }

        yield return null;
    }

    private void OnDisable()
    {
        _gunManager.weapon.noFriendsInWar = false;
    }
}
