using System.Collections;
using FMODUnity;
using UnityEngine;

public class CannonFodder : SkillBase
{
    [SerializeField] private float duration = 12f;
    [SerializeField] private float cooldownThisSkill = 25f;
    [Space]
    [SerializeField] private Transform playerTransform;

    public override IEnumerator Execute()
    {
        cooldown = cooldownThisSkill;
        RuntimeManager.PlayOneShotAttached(skillInfo.activateSkillSound, gameObject);

        EnemyMovement[] enemies = FindObjectsByType<EnemyMovement>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (EnemyMovement enemy in enemies)
        {
            enemy.Player = playerTransform;
        }

        yield return new WaitForSeconds(duration);

        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);

        foreach (EnemyMovement enemy in enemies)
        {
            if(Random.Range(0, 1) > 0.5f)
                enemy.Player = players[1].transform;
            else
                enemy.Player = players[0].transform;
        }
    }
}
