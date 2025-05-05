using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "RhinoCharge Skill", menuName = "Enemies/Skills/RhinoCharge Skill")]
public class RhinoChargeSkill : SkillScriptableObject
{
    public float chargeSpeed = 20f;
    public float chargeDuration = 2f;
    public float minActivationDistance = 2f;
    public float maxActivationDistance = 3f;


    public override SkillScriptableObject scaleUpForLevel(ScalingScriptableObject scaling, int level)
    {
        RhinoChargeSkill scaledSkill = CreateInstance<RhinoChargeSkill>();

        ScaleUpBaseValuesForLevel(scaledSkill, scaling, level);
        scaledSkill.chargeSpeed = chargeSpeed + Mathf.FloorToInt(chargeSpeed * scaling.damageCurve.Evaluate(level));
        scaledSkill.chargeDuration = chargeDuration;
        scaledSkill.minActivationDistance = minActivationDistance;
        scaledSkill.maxActivationDistance = maxActivationDistance;

        return scaledSkill;
    }

    public override bool CanUseSkill(Enemy enemy, PlayerController player, int level)
    {
        return base.CanUseSkill(enemy, player, level)
            && Vector3.Distance(enemy.transform.position, player.transform.position) >= minActivationDistance
            && Vector3.Distance(enemy.transform.position, player.transform.position) <= maxActivationDistance;
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

        // Find the correct collider by tag
        BoxCollider chargeCollider = null;
        foreach (BoxCollider col in enemy.GetComponentsInChildren<BoxCollider>())
        {
            if (col.CompareTag("ChargeCollider"))
            {
                chargeCollider = col;
                break;
            }
        }

        if (rb == null || agent == null || chargeCollider == null)
        {
            Debug.LogError("Enemy is missing required components: Rigidbody, NavMeshAgent, or ChargeCollider.");
            yield break;
        }

        chargeCollider.enabled = false; // Disable the charge collider initially
        chargeCollider.GetComponent<ChargeDamage>().Damage = damage; // Set the damage value


        // Disable NavMeshAgent and enable Rigidbody
        agent.enabled = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        enemy.Movement.enabled = false;

        // Calculate charge direction
        Vector3 chargeDirection = (player.transform.position - enemy.transform.position).normalized;
        chargeDirection.y = 0; // Keep the charge direction horizontal

        // Rotate the enemy to face the charge direction
        Quaternion targetRotation = Quaternion.LookRotation(chargeDirection);
        enemy.transform.rotation = targetRotation;

        // Set enemy state and animation
        enemy.Movement.State = EnemyState.UsingAbilty;
        enemy.Animator.SetTrigger(Enemy.SKILL_TRIGGER);

        // **Charge preparation phase**
        float chargePreparationTime = 0.5f; // Time to prepare before charging
        Debug.Log("Enemy is preparing to charge...");
        yield return new WaitForSeconds(chargePreparationTime);

        // Enable the charge collider
        chargeCollider.enabled = true;

        // **Charge Movement phase**
        float elapsedTime = 0f;

        while (elapsedTime < chargeDuration)
        {
            rb.AddForce(enemy.transform.forward * chargeSpeed, ForceMode.Acceleration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Disable the charge collider after the charge
        chargeCollider.enabled = false;

        // Stop the charge
        rb.linearVelocity = Vector3.zero;

        // Re-enable NavMeshAgent and disable Rigidbody
        rb.isKinematic = true;
        rb.useGravity = false;
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

        ResetSkillState(enemy); // Reset the skill state for this enemy
    }

    public override bool MultiCanUseSkill(EnemyMulti enemy, PlayerControllerMulti player, int level)
    {
        return base.MultiCanUseSkill(enemy, player, level)
            && Vector3.Distance(enemy.transform.position, player.transform.position) >= minActivationDistance
            && Vector3.Distance(enemy.transform.position, player.transform.position) <= maxActivationDistance;
    }

    public override void MultiUseSkill(EnemyMulti enemy, PlayerControllerMulti player)
    {
        base.MultiUseSkill(enemy, player);

        Debug.Log($"Enemy {enemy.name} is using Rhino Charge skill on {player.name}.");
        enemy.StartCoroutine(MultiPerformCharge(enemy, player));
    }

    private IEnumerator MultiPerformCharge(EnemyMulti enemy, PlayerControllerMulti player)
    {
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        NavMeshAgent agent = enemy.Agent;

        // Find the correct collider by tag
        BoxCollider chargeCollider = null;
        foreach (BoxCollider col in enemy.GetComponentsInChildren<BoxCollider>())
        {
            if (col.CompareTag("ChargeCollider"))
            {
                chargeCollider = col;
                break;
            }
        }

        if (rb == null || agent == null || chargeCollider == null)
        {
            Debug.LogError("Enemy is missing required components: Rigidbody, NavMeshAgent, or ChargeCollider.");
            yield break;
        }

        chargeCollider.enabled = false; // Disable the charge collider initially
        chargeCollider.GetComponent<MultiChargeDamage>().Damage = damage; // Set the damage value


        // Disable NavMeshAgent and enable Rigidbody
        agent.enabled = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        enemy.Movement.enabled = false;

        // Calculate charge direction
        Vector3 chargeDirection = (player.transform.position - enemy.transform.position).normalized;
        chargeDirection.y = 0; // Keep the charge direction horizontal

        // Rotate the enemy to face the charge direction
        Quaternion targetRotation = Quaternion.LookRotation(chargeDirection);
        enemy.transform.rotation = targetRotation;

        // Set enemy state and animation
        enemy.Movement.State = EnemyState.UsingAbilty;
        enemy.Animator.SetTrigger(EnemyMulti.SKILL_TRIGGER);

        // **Charge preparation phase**
        float chargePreparationTime = 0.5f; // Time to prepare before charging
        Debug.Log("Enemy is preparing to charge...");
        yield return new WaitForSeconds(chargePreparationTime);

        // Enable the charge collider
        chargeCollider.enabled = true;

        // **Charge Movement phase**
        float elapsedTime = 0f;

        while (elapsedTime < chargeDuration)
        {
            rb.AddForce(enemy.transform.forward * chargeSpeed, ForceMode.Acceleration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Disable the charge collider after the charge
        chargeCollider.enabled = false;

        // Stop the charge
        rb.linearVelocity = Vector3.zero;

        // Re-enable NavMeshAgent and disable Rigidbody
        rb.isKinematic = true;
        rb.useGravity = false;
        enemy.Movement.enabled = true;
        agent.enabled = true;

        // Warp the enemy back to the NavMesh
        if (NavMesh.SamplePosition(enemy.transform.position, out NavMeshHit hit, 1f, agent.areaMask))
        {
            agent.Warp(hit.position);
            enemy.Movement.State = EnemyState.Chase;
        }
        else
        {
            Debug.LogWarning("Failed to warp enemy back to NavMesh.");
        }

        MultiResetSkillState(enemy); // Reset the skill state for this enemy
    }
}