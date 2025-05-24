using System.Collections;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "Bomb Skill", menuName = "Enemies/Skills/Bomb Skill")]
public class BombSkill : SkillScriptableObject
{
    public int bombsToShoot = 1;
    public float delay = 1f;
    public PoolableObject prefab;
    public NetworkObject multiPrefab;
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
        bool hasLineOfSight = HasLineOfSight(enemy, player.transform);

        //Debug.Log($"CanUseSkill - Enemy: {enemy.name}, BaseCondition: {baseCondition}, InRange: {inRange}, HasLineOfSight: {hasLineOfSight}");

        return baseCondition && inRange && hasLineOfSight;
    }

    public override void UseSkill(Enemy enemy, PlayerController player)
    {
        //Debug.Log($"UseSkill - Enemy: {enemy.name}, isActivating: {isActivating}, Time: {Time.time}, useTime: {useTime}");

        base.UseSkill(enemy, player);

        enemy.StartCoroutine(ShootBomb(enemy, player));
    }

    private IEnumerator ShootBomb(Enemy enemy, PlayerController player)
    {
        Debug.Log($"ShootBomb - Enemy: {enemy.name}, Starting skill activation.");

        WaitForSeconds wait = new WaitForSeconds(delay);

        DisableEnemyMovement(enemy);
        enemy.Movement.State = EnemyState.UsingAbilty;

        /*
        for (float time = 0; time < 1f; time += Time.deltaTime * 2f)
        {
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, Quaternion.LookRotation(player.transform.position - enemy.transform.position), enemy.Agent.angularSpeed *time);
            yield return null;
        }
        */

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

        //Debug.Log($"Bomb instantiated: {instance.name}, Parent: {instance.transform.parent?.name ?? "None"}");

        instance.transform.position = enemy.transform.position + bulletSpawnOffSet;
        instance.transform.rotation = enemy.Agent.transform.rotation;

        BombBullet bomb = instance.GetComponent<BombBullet>();
        bomb.Spawn(enemy.transform.forward, explosionDamage, player.transform);
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

        bool baseCondition = base.MultiCanUseSkill(enemy, player, level);
        bool inRange = Vector3.Distance(enemy.transform.position, player.transform.position) <= range;
        bool hasLineOfSight = MultiHasLineOfSight(enemy, player.transform);

        //Debug.Log($"CanUseSkill - Enemy: {enemy.name}, BaseCondition: {baseCondition}, InRange: {inRange}, HasLineOfSight: {hasLineOfSight}");

        return baseCondition && inRange && hasLineOfSight;
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

        for (int i = 0; i < bombsToShoot; i++)
        {
            enemy.Animator.SetTrigger(EnemyMulti.SKILL_TRIGGER);
            MultiShootingBombLogic(enemy, player);
            yield return wait;
        }

        MultiResetSkillState(enemy); // Reset the skill state for this enemy

        MultiEnableEnemyMovement(enemy);
        enemy.Movement.State = EnemyState.Chase;
    }

    private void MultiShootingBombLogic(EnemyMulti enemy, PlayerControllerMulti player)
    {
       // ObjectPoolMulti pool = ObjectPoolMulti.CreateInstance(multiPrefab, 10);
       // PoolableObjectMulti instance = pool.GetObject();
        
        NetworkObjectPool networkObjectPool = NetworkObjectPool.Singleton.GetComponent<NetworkObjectPool>();
        NetworkObject netObject = networkObjectPool.GetNetworkObject(multiPrefab.gameObject, Vector3.zero, Quaternion.Euler(0, 0, 0));
        if (!netObject.IsSpawned) netObject.Spawn();
        MultiBulletEnemy bullet = netObject.GetComponent<MultiBombBullet>();
        bullet.OnCollision += ReturnBulletEnemy;

        Debug.Log($"Bomb instantiated: {netObject.name}, Parent: {netObject.transform.parent?.name ?? "None"}");

        //netObject.transform.SetParent(enemy.transform, false);
        Vector3 bulletPos = enemy.transform.position + bulletSpawnOffSet;
        Quaternion bulletRot = enemy.Agent.transform.rotation;
        bullet.Spawn(enemy.transform.forward, explosionDamage, player.transform, bulletPos, bulletRot);
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
            IDamageableMulti damageable;

            if (hit.collider.TryGetComponent<IDamageableMulti>(out damageable))
            {
                //Debug.Log("Line of sight to target: " + (damageable.GetTransform() == hit.transform));
                return damageable.GetTransform() == target;
            }
        }

        return false;
    }
    public void ReturnBulletEnemy(MultiBulletEnemy bullet)
    {
        NetworkObjectPool networkObjectPool = NetworkObjectPool.Singleton.GetComponent<NetworkObjectPool>();
        NetworkObject netObj = bullet.gameObject.GetComponent<NetworkObject>();
        networkObjectPool.ReturnNetworkObject(netObj, multiPrefab.gameObject);
        bullet.OnCollision -= ReturnBulletEnemy;
    }
}
