using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class Blink : SkillBase
{
    [SerializeField] private float blinkSpeed = 100f;
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private float cooldownThisSkill = 5f;
    [SerializeField] private Transform freeLookCamera;

    private Rigidbody rb;
    private Animator animator;

    private void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        animator = GetComponentInParent<Animator>();
        cooldown = cooldownThisSkill;
    }
    public override IEnumerator Execute()
    {
        animator.applyRootMotion = false;
        rb.useGravity = false;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(freeLookCamera.forward * blinkSpeed, ForceMode.Impulse);

        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        animator.applyRootMotion = true;
        rb.useGravity = true;
    }
}
