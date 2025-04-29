using UnityEngine;

public class SkillScriptableObject : ScriptableObject
{
    public float cooldown = 10f;
    public int damage = 5;
    public int unlocklevel = 1;

    public bool isActivating;

    public float useTime;

    public virtual SkillScriptableObject scaleUpForLevel(ScalingScriptableObject scaling, int level)
    {
        SkillScriptableObject scaledSkill = CreateInstance<SkillScriptableObject>();

        ScaleUpBaseValuesForLevel(scaledSkill, scaling, level);
        return scaledSkill;

    }

    protected virtual void ScaleUpBaseValuesForLevel(SkillScriptableObject scaledSkill, ScalingScriptableObject scaling, int level)
    {
        scaledSkill.name = name;

        scaledSkill.cooldown = cooldown;
        scaledSkill.damage = damage + Mathf.FloorToInt(damage * scaling.damageCurve.Evaluate(level));
        scaledSkill.unlocklevel = unlocklevel;
    }

    public virtual void UseSkill(Enemy enemy, PlayerController player)
    {
        isActivating = true;
    }
    public virtual void MultiUseSkill(EnemyMulti enemy, PlayerControllerMulti player)
    {
        isActivating = true;
    }

    public virtual bool CanUseSkill(Enemy enemy, PlayerController player, int level){
        return !isActivating && level >= unlocklevel && useTime + cooldown < Time.time;
    }    
    public virtual bool MultiCanUseSkill(EnemyMulti enemy, PlayerControllerMulti player, int level){
        return !isActivating && level >= unlocklevel && useTime + cooldown < Time.time;
    }

    protected void DisableEnemyMovement(Enemy enemy)
    {
        enemy.enabled = false;
        enemy.Agent.enabled = false;
        enemy.Movement.enabled = false;
    }

    protected void EnableEnemyMovement(Enemy enemy)
    {
        enemy.enabled = true;
        enemy.Agent.enabled = true;
        enemy.Movement.enabled = true;
    }

    protected void MultiDisableEnemyMovement(EnemyMulti enemy)
    {
        enemy.enabled = false;
        enemy.Agent.enabled = false;
        enemy.Movement.enabled = false;
    }

    protected void MultiEnableEnemyMovement(EnemyMulti enemy)
    {
        enemy.enabled = true;
        enemy.Agent.enabled = true;
        enemy.Movement.enabled = true;
    }
}
