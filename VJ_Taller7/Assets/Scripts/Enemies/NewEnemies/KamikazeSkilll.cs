using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Kamikaze Skill", menuName = "Enemies/Skills/Kamikaze Skill")]
public class KamikazeSkilll : SkillScriptableObject
{
    public int bombsToShoot = 1;
    public float delay = 1f;
    public PoolableObject prefab;
    public PoolableObjectMulti multiPrefab;
    public LayerMask lineOfSightLayerMask;
    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    public int explosionDamage = 10;
    public float range = 10f;
    public float bombSize = 0.8f;
    public Vector3 bulletSpawnOffSet = new Vector3(0, 1, 0);

    public override SkillScriptableObject scaleUpForLevel(ScalingScriptableObject scaling, int level)
    {
        BombSkill scaledSkill = CreateInstance<BombSkill>();

        ScaleUpBaseValuesForLevel(scaledSkill, scaling, level);
        scaledSkill.bombsToShoot = bombsToShoot;
        scaledSkill.delay = delay;
        scaledSkill.prefab = prefab;
        scaledSkill.multiPrefab = multiPrefab;
        scaledSkill.lineOfSightLayerMask = lineOfSightLayerMask;
        scaledSkill.explosionRadius = explosionRadius;
        scaledSkill.explosionForce = explosionForce + Mathf.FloorToInt(explosionForce * scaling.damageCurve.Evaluate(level));
        scaledSkill.explosionDamage = explosionDamage + Mathf.FloorToInt(explosionDamage * scaling.damageCurve.Evaluate(level));
        scaledSkill.range = range;
        scaledSkill.bombSize = bombSize;
        scaledSkill.bulletSpawnOffSet = bulletSpawnOffSet;

        return scaledSkill;
    }

    public override bool CanUseSkill(Enemy enemy, PlayerController player, int level)
    {
        bool baseCondition = base.CanUseSkill(enemy, player, level);
        bool inRange = Vector3.Distance(enemy.transform.position, player.transform.position) <= range;

        //Debug.Log($"CanUseSkill - Enemy: {enemy.name}, BaseCondition: {baseCondition}, InRange: {inRange}, HasLineOfSight: {hasLineOfSight}");

        return baseCondition && inRange;
    }

    public override void UseSkill(Enemy enemy, PlayerController player)
    {
        base.UseSkill(enemy, player);

        enemy.StartCoroutine(ShootBomb(enemy, player));
    }

    private IEnumerator ShootBomb(Enemy enemy, PlayerController player)
    {
        WaitForSeconds wait = new WaitForSeconds(delay);

        DisableEnemyMovement(enemy);
        enemy.Movement.State = EnemyState.UsingAbilty;

        for(int i = 0; i < bombsToShoot; i++)
        {
            enemy.Animator.SetTrigger(Enemy.SKILL_TRIGGER);
            ShootingBombLogic(enemy, player);
            yield return wait;
        }

        ResetSkillState(enemy); // Reset the skill state for this enemy

        EnableEnemyMovement(enemy);
        enemy.Movement.State = EnemyState.Chase;

    }

    private void ShootingBombLogic(Enemy enemy, PlayerController player)
    {
        ObjectPool pool = ObjectPool.CreateInstance(prefab, 10);
        PoolableObject instance = pool.GetObject();

        Debug.Log($"Bomb instantiated: {instance.name}, Parent: {instance.transform.parent?.name ?? "None"}");

        instance.transform.SetParent(enemy.transform, false);
        instance.transform.localPosition = bulletSpawnOffSet;
        instance.transform.rotation = enemy.Agent.transform.rotation;

        BombBullet bomb = instance.GetComponent<BombBullet>();
        bomb.Spawn(enemy.transform.forward, explosionDamage, player.transform);
    }

    public override bool MultiCanUseSkill(EnemyMulti enemy, PlayerControllerMulti player, int level)
    {
        return base.MultiCanUseSkill(enemy, player, level)
            && Vector3.Distance(enemy.transform.position, player.transform.position) <= range;
    }

    public override void MultiUseSkill(EnemyMulti enemy, PlayerControllerMulti player)
    {
        base.MultiUseSkill(enemy, player);

        enemy.StartCoroutine(MultiShootBomb(enemy, player));
    }

    private IEnumerator MultiShootBomb(EnemyMulti enemy, PlayerControllerMulti player)
    {
        WaitForSeconds wait = new WaitForSeconds(delay);

        MultiDisableEnemyMovement(enemy);
        enemy.Movement.State = EnemyState.UsingAbilty;

        for (float time = 0; time < 1f; time += Time.deltaTime * 2f)
        {
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, Quaternion.LookRotation(player.transform.position - enemy.transform.position), time);
            yield return null;
        }

        ObjectPoolMulti pool = ObjectPoolMulti.CreateInstance(multiPrefab, 10);
        PoolableObjectMulti instance = pool.GetObject();

        enemy.Animator.SetTrigger(Enemy.SKILL_TRIGGER);

        instance.transform.SetParent(enemy.transform, false);
        instance.transform.localPosition = bulletSpawnOffSet;
        instance.transform.rotation = enemy.Agent.transform.rotation;

        MultiBombBullet bomb = instance.GetComponent<MultiBombBullet>();
        bomb.Spawn(enemy.transform.forward, explosionDamage, player.transform);

        yield return wait;

        //useTime = Time.time;
        //isActivating = false;

        MultiEnableEnemyMovement(enemy);
        enemy.Movement.State = EnemyState.Chase;

    }
}
