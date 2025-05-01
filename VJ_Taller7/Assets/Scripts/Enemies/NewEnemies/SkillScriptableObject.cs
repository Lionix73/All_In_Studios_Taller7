using UnityEngine;
using System.Collections.Generic;

public class SkillScriptableObject : ScriptableObject
{
    public float cooldown = 10f;
    public int damage = 5;
    public int unlocklevel = 1;

    private Dictionary<Enemy, bool> isActivatingPerEnemy = new Dictionary<Enemy, bool>();
    private Dictionary<Enemy, float> useTimePerEnemy = new Dictionary<Enemy, float>();


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
        if (!isActivatingPerEnemy.ContainsKey(enemy))
        {
            isActivatingPerEnemy[enemy] = false;
        }

        isActivatingPerEnemy[enemy] = true;
    }

    public virtual void MultiUseSkill(EnemyMulti enemy, PlayerControllerMulti player)
    {
        //isActivating = true;
    }

    public virtual bool CanUseSkill(Enemy enemy, PlayerController player, int level)
    {
        if (!isActivatingPerEnemy.ContainsKey(enemy))
        {
            isActivatingPerEnemy[enemy] = false;
        }

        if (!useTimePerEnemy.ContainsKey(enemy))
        {
            useTimePerEnemy[enemy] = 0f;
        }

        bool isActivating = isActivatingPerEnemy[enemy];
        float useTime = useTimePerEnemy[enemy];

        return !isActivating && level >= unlocklevel && useTime + cooldown < Time.time;
    }

    public virtual bool MultiCanUseSkill(EnemyMulti enemy, PlayerControllerMulti player, int level){
        //return !isActivating && level >= unlocklevel && useTime + cooldown < Time.time;
        return true; // Placeholder para yo testear HAY QUE CAMBIAR ESTO
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

    protected void ResetSkillState(Enemy enemy)
    {
        if (isActivatingPerEnemy.ContainsKey(enemy))
        {
            isActivatingPerEnemy[enemy] = false;
        }

        if (useTimePerEnemy.ContainsKey(enemy))
        {
            useTimePerEnemy[enemy] = Time.time;
        }
        else
        {
            useTimePerEnemy[enemy] = Time.time;
        }
    }

    protected virtual void OnDisable()
    {
        // Clear the dictionaries to avoid memory leaks
        isActivatingPerEnemy.Clear();
        useTimePerEnemy.Clear();
        Debug.Log("SkillScriptableObject: Cleared enemy-specific states on disable.");
    }
}
