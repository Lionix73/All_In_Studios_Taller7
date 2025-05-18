using System.Collections;
using UnityEngine;

public class YourChargedWeapon : PassiveSkillBase
{
    [SerializeField][Range(1, 100)] private int healthIncreasePercentage = 50;
    [SerializeField][Range(1, 100)] private int damageIncreasePercentage = 25;
    [SerializeField] private float duration = 30f;
    [SerializeField] private GameObject player;

    private GameObject[] _players;
    private GunManager _gunManager;
    private Health _friendHealth;
    private float _orignalMaxHealth;
    private int _orignalDamage;
    private int _friendPlayerIndex;

    private void Start()
    {
        _players = GameObject.FindGameObjectsWithTag("Player");

        if (_players.Length < 2) return;

        _friendPlayerIndex = player == _players[0] ? 1 : 0;

        _friendHealth = _players[_friendPlayerIndex].GetComponent<Health>();
        _gunManager = _players[_friendPlayerIndex].GetComponentInParent<GunManager>();

        _orignalMaxHealth = _friendHealth.GetMaxHeath;
        _orignalDamage = _gunManager.CurrentGun.Damage;
    }

    public override void CheckCondition()
    {
        if (_players.Length < 2) return;

        if (player.GetComponent<Health>().isDead)
        {
            StartCoroutine(Execute());
        }
    }

    public override IEnumerator Execute()
    {
        _players[_friendPlayerIndex].GetComponent<ThisObjectSounds>().PlaySound("YourChargedWeapon");

        float incrementedHealth = _orignalMaxHealth * ((float)healthIncreasePercentage/100);
        _friendHealth.ScaleHealth(incrementedHealth);

        int incrementedDamage = (int)(_orignalDamage * (1 + (damageIncreasePercentage/100)));
        _gunManager.CurrentGun.Damage = incrementedDamage;

        yield return new WaitForSeconds(duration);

        float ExtraHealth = _friendHealth.GetMaxHeath - _orignalMaxHealth;
        _friendHealth.ScaleHealth(-ExtraHealth);
        _gunManager.CurrentGun.Damage = _orignalDamage;
    }
}
