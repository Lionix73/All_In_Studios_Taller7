using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Kamikaze Skill", menuName = "Enemies/Skills/Kamikaze Skill")]
public class KamikazeSkilll : SkillScriptableObject
{
    public float delay = 1f;
    public PoolableObject prefab;
    public LayerMask lineOfSightLayerMask;
    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    public int explosionDamage = 10;
    public float range = 10f;
    public float bombSize = 0.8f;
    public Vector3 bulletSpawnOffSet = new Vector3(0, 1, 0);

    private bool bombLaunched = false;

    public override SkillScriptableObject scaleUpForLevel(ScalingScriptableObject scaling, int level)
    {
        BombSkill scaledSkill = CreateInstance<BombSkill>();

        ScaleUpBaseValuesForLevel(scaledSkill, scaling, level);
        scaledSkill.delay = delay;
        scaledSkill.prefab = prefab;
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
        return base.CanUseSkill(enemy, player, level)
            && Vector3.Distance(enemy.transform.position, player.transform.position) <= range;
    }

    public override void UseSkill(Enemy enemy, PlayerController player)
    {
        base.UseSkill(enemy, player);

        if (bombLaunched) return;
        enemy.StartCoroutine(ShootBomb(enemy, player));
    }

    private IEnumerator ShootBomb(Enemy enemy, PlayerController player)
    {
        WaitForSeconds wait = new WaitForSeconds(delay);

        enemy.Animator.SetBool(EnemyMovement.IsWalking, false);

        DisableEnemyMovement(enemy);
        enemy.Movement.State = EnemyState.UsingAbilty;

        for (float time = 0; time < 1f; time += Time.deltaTime * 2f)
        {
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, Quaternion.LookRotation(player.transform.position - enemy.transform.position), time);
            yield return null;
        }

        ObjectPool pool = ObjectPool.CreateInstance(prefab, 10);
        PoolableObject instance = pool.GetObject();

        enemy.Animator.SetTrigger(Enemy.ATTACK_TRIGGER);

        instance.transform.SetParent(enemy.transform, false);
        instance.transform.localPosition = bulletSpawnOffSet;
        instance.transform.rotation = enemy.Agent.transform.rotation;

        BombBullet bomb = instance.GetComponent<BombBullet>();
        bomb.Spawn(enemy.transform.forward, explosionDamage, player.transform);

        bombLaunched = true;

        yield return wait;

        useTime = Time.time;
        isActivating = false;

        EnableEnemyMovement(enemy);
        enemy.Movement.State = EnemyState.Chase;

    }
}
