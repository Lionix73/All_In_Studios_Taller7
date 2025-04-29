using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "RhinoCharge Skill", menuName = "Enemies/Skills/RhinoCharge Skill")]
public class RhinoChargeSkill : SkillScriptableObject
{
    public float chargeSpeed = 20f;
    public float chargeDuration = 2f;
    public float minActivationDistance = 5f;

    public override SkillScriptableObject scaleUpForLevel(ScalingScriptableObject scaling, int level)
    {
        RhinoChargeSkill scaledSkill = CreateInstance<RhinoChargeSkill>();

        ScaleUpBaseValuesForLevel(scaledSkill, scaling, level);
        scaledSkill.chargeSpeed = chargeSpeed + Mathf.FloorToInt(chargeSpeed * scaling.damageCurve.Evaluate(level));
        scaledSkill.chargeDuration = chargeDuration;
        scaledSkill.minActivationDistance = minActivationDistance;

        return scaledSkill;
    }

    public override bool CanUseSkill(Enemy enemy, PlayerController player, int level)
    {
        return base.CanUseSkill(enemy, player, level)
            && Vector3.Distance(enemy.transform.position, player.transform.position) >= minActivationDistance;
    }

    public override void UseSkill(Enemy enemy, PlayerController player)
    {
        base.UseSkill(enemy, player);

        Debug.Log($"Enemy {enemy.name} is using Rhino Charge skill on {player.name}.");
        enemy.StartCoroutine(PerformCharge(enemy, player));
    }

    private IEnumerator PerformCharge(Enemy enemy, PlayerController player)
    {
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        NavMeshAgent agent = enemy.Agent;

        if (rb == null || agent == null)
        {
            Debug.LogError("Enemy is missing required components: Rigidbody or NavMeshAgent.");
            yield break;
        }

        // Disable NavMeshAgent and enable Rigidbody
        agent.enabled = false;
        rb.isKinematic = false;
        enemy.Movement.enabled = false;

        // Calculate charge direction
        Vector3 chargeDirection = (player.transform.position - enemy.transform.position).normalized;

        // Set enemy state and animation
        enemy.Movement.State = EnemyState.UsingAbilty;
        enemy.Animator.SetTrigger(Enemy.ATTACK_TRIGGER);

        float elapsedTime = 0f;

        while (elapsedTime < chargeDuration)
        {
            rb.linearVelocity = chargeDirection * chargeSpeed;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Stop the charge
        rb.linearVelocity = Vector3.zero;

        // Re-enable NavMeshAgent and disable Rigidbody
        rb.isKinematic = true;
        enemy.Movement.enabled = true;
        agent.enabled = true;

        // Warp the enemy back to the NavMesh
        if(NavMesh.SamplePosition(enemy.transform.position, out NavMeshHit hit, 1f, agent.areaMask))
        {
            agent.Warp(hit.position);
            enemy.Movement.State = EnemyState.Chase;
        }
        else
        {
            Debug.LogWarning("Failed to warp enemy back to NavMesh.");
        }

        // Reset state and cooldown
        useTime = Time.time;
        isActivating = false;
    }
}