using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Bomb Skill", menuName = "Enemies/Skills/Bomb Skill")]
public class BombSkill : SkillScriptableObject
{
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
        return base.CanUseSkill(enemy, player, level)
            && Vector3.Distance(enemy.transform.position, player.transform.position) <= range
            && HasLineOfSight(enemy, player.transform);
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

        for (float time = 0; time < 1f; time += Time.deltaTime * 2f)
        {
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, Quaternion.LookRotation(player.transform.position - enemy.transform.position), enemy.Agent.angularSpeed *time);
            yield return null;
        }

        enemy.Animator.SetTrigger(Enemy.SKILL_TRIGGER);

        ObjectPool pool = ObjectPool.CreateInstance(prefab, 5);
        Debug.Log("Bomba Activada");
        PoolableObject instance = pool.GetObject();

        instance.transform.SetParent(enemy.transform, false);
        instance.transform.localPosition = bulletSpawnOffSet;
        instance.transform.rotation = enemy.Agent.transform.rotation;

        BombBullet bomb = instance.GetComponent<BombBullet>();
        bomb.Spawn(enemy.transform.forward, explosionDamage, player.transform);

        yield return wait;
        
        useTime = Time.time;
        isActivating = false;

        EnableEnemyMovement(enemy);
        enemy.Movement.State = EnemyState.Chase;
    }

    private bool HasLineOfSight(Enemy enemy, Transform target)
    {
        Vector3 origin = enemy.transform.position + bulletSpawnOffSet;
        Vector3 direction = target.position + bulletSpawnOffSet - origin;

        // Visualize the sphere cast
        Debug.DrawLine(origin, origin + direction.normalized * range, Color.red);
        Debug.DrawRay(origin, direction.normalized * bombSize, Color.green);

        if (Physics.SphereCast(origin, bombSize, direction.normalized, out RaycastHit hit, range, lineOfSightLayerMask))
        {
            IDamageable damageable;

            if(hit.collider.TryGetComponent<IDamageable>(out damageable))
            {
                //Debug.Log("Line of sight to target: " + (damageable.GetTransform() == hit.transform));
                return damageable.GetTransform() == target  ;
            }
        }

        return false;
    }

    public override bool MultiCanUseSkill(EnemyMulti enemy, PlayerControllerMulti player, int level)
    {
        return base.MultiCanUseSkill(enemy, player, level)
            && Vector3.Distance(enemy.transform.position, player.transform.position) <= range
            && MultiHasLineOfSight(enemy, player.transform);
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
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, Quaternion.LookRotation(player.transform.position - enemy.transform.position), enemy.Agent.angularSpeed * time);
            yield return null;
        }

        enemy.Animator.SetTrigger(EnemyMulti.ATTACK_TRIGGER);

        ObjectPoolMulti pool = ObjectPoolMulti.CreateInstance(multiPrefab, 5);
        Debug.Log("Bomba Activada");
        PoolableObjectMulti instance = pool.GetObject();

        instance.transform.SetParent(enemy.transform, false);
        instance.transform.localPosition = bulletSpawnOffSet;
        instance.transform.rotation = enemy.Agent.transform.rotation;

        MultiBombBullet bomb = instance.GetComponent<MultiBombBullet>();
        bomb.Spawn(enemy.transform.forward, explosionDamage, player.transform);

        yield return wait;

        useTime = Time.time;
        isActivating = false;

        MultiEnableEnemyMovement(enemy);
        enemy.Movement.State = EnemyState.Chase;
    }
    private bool MultiHasLineOfSight(EnemyMulti enemy, Transform target)
    {
        Vector3 origin = enemy.transform.position + bulletSpawnOffSet;
        Vector3 direction = target.position + bulletSpawnOffSet - origin;

        // Visualize the sphere cast
        Debug.DrawLine(origin, origin + direction.normalized * range, Color.red);
        Debug.DrawRay(origin, direction.normalized * bombSize, Color.green);

        if (Physics.SphereCast(origin, bombSize, direction.normalized, out RaycastHit hit, range, lineOfSightLayerMask))
        {
            IDamageable damageable;

            if (hit.collider.TryGetComponent<IDamageable>(out damageable))
            {
                //Debug.Log("Line of sight to target: " + (damageable.GetTransform() == hit.transform));
                return damageable.GetTransform() == target;
            }
        }

        return false;
    }
}
