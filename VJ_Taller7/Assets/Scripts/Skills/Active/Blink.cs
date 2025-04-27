using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class Blink : SkillBase
{
    [SerializeField] private float blinkSpeed = 100f;
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private float cooldownThisSkill = 5f;
    
    private Transform freeLookCamera;
    private Rigidbody rb;
    private Animator animator;

    private void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        animator = GetComponentInParent<Animator>();
        freeLookCamera = GameObject.FindWithTag("MainCamera").GetComponent<Transform>();
    }
    public override IEnumerator Execute()
    {
        cooldown = cooldownThisSkill;

        animator.applyRootMotion = false;
        rb.useGravity = false;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(freeLookCamera.forward * blinkSpeed, ForceMode.Impulse);

        yield return new WaitForSeconds(duration);

        animator.applyRootMotion = true;
        rb.useGravity = true;
    }
}
