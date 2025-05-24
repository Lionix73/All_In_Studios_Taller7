using System.Collections;
using UnityEngine;

public class MultiYourChargedWeapon : MultiPassiveSkillBase
{
    [SerializeField][Range(1, 100)] private int healthIncreasePercentage = 50;
    [SerializeField][Range(1, 100)] private int damageIncreasePercentage = 25;
    [SerializeField] private float duration = 30f;
    [SerializeField] private GameObject player;

    private GameObject[] _players;
    private GunManagerMulti2 _gunManager;
    private HealthMulti _friendHealth;
    private float _orignalMaxHealth;
    private int _orignalDamage;
    private int _friendPlayerIndex;

    private void Start()
    {
        _players = GameObject.FindGameObjectsWithTag("Player");
        _friendPlayerIndex = player == _players[0] ? 1 : 0;

        _friendHealth = _players[_friendPlayerIndex].GetComponent<HealthMulti>();
        _gunManager = _players[_friendPlayerIndex].GetComponentInParent<GunManagerMulti2>();

        _orignalMaxHealth = (int)_friendHealth.MaxHealth;
        _orignalDamage = _gunManager.weapon.Damage;
    }

    public override void CheckCondition()
    {
        if (player.GetComponent<HealthMulti>().IsDead)
        {
            StartCoroutine(Execute());
        }
    }

    public override IEnumerator Execute()
    {
        float incrementedHealth = _orignalMaxHealth * (healthIncreasePercentage/100);
        _friendHealth.ScaleHealth(incrementedHealth);

        int incrementedDamage = (int)(_orignalDamage * (1 + (damageIncreasePercentage/100)));
        _gunManager.CurrentGun.Damage = incrementedDamage;

        yield return new WaitForSeconds(duration);

        float originalHealth = (int)_friendHealth.MaxHealth - _orignalMaxHealth;
        _friendHealth.ScaleHealth(-originalHealth);
        _gunManager.CurrentGun.Damage = _orignalDamage;
    }
}
