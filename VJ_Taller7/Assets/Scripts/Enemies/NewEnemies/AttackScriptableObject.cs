using UnityEngine;

[CreateAssetMenu(fileName = "Attack ScriptableObject", menuName = "Enemies/Attack ScriptableObject")]
public class AttackScriptableObject : ScriptableObject
{
    public bool isRanged = false;
    public int damage = 5;
    public float attackRadius = 1f;
    public float attackDelay = 1f;

    //Ranged
    public BulletEnemie bulletPrefab;
    public Vector3 bulletSpawnOffSet = new Vector3(0, 1, 0);
    public LayerMask lineOfSightLayer;

    public AttackScriptableObject ScaleUpLevel(ScalingScriptableObject scaling, int level){
        AttackScriptableObject scaledAttack = CreateInstance<AttackScriptableObject>();

        scaledAttack.isRanged = isRanged;
        scaledAttack.damage = Mathf.FloorToInt(damage * scaling.damageCurve.Evaluate(level));
        scaledAttack.attackRadius = attackRadius;
        scaledAttack.attackDelay = attackDelay;

        scaledAttack.bulletPrefab = bulletPrefab;
        scaledAttack.bulletSpawnOffSet = bulletSpawnOffSet;
        scaledAttack.lineOfSightLayer = lineOfSightLayer;

        return scaledAttack;
    }

    public void SetUpEnemey(Enemy enemy){
        (enemy.AttackRadius.sphereCollider == null ? enemy.AttackRadius.GetComponent<SphereCollider>() : enemy.AttackRadius.sphereCollider).radius = attackRadius;
        enemy.AttackRadius.AttackDelay = attackDelay;
        enemy.AttackRadius.Damage = damage;

        if(isRanged){
            RangedAttackRadius rangedAttackRadius = enemy.AttackRadius.GetComponent<RangedAttackRadius>();
            
            rangedAttackRadius.BulletPrefab = bulletPrefab;
            rangedAttackRadius.BulletSpawnOffset = bulletSpawnOffSet;
            rangedAttackRadius.Mask = lineOfSightLayer;

            rangedAttackRadius.CreateBulletPool();
        }
    }
}
