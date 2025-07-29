using System.Collections;
using FMODUnity;
using UnityEngine;

public class Blink : SkillBase
{
    [SerializeField] private float blinkSpeed = 100f;
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private float cooldownThisSkill = 5f;
    [SerializeField] private MeshTrail dashVFX;

    private Transform freeLookCamera;
    private Rigidbody rb;
    private Animator animator;

    private void Start()
    {
        rb = GetComponentInParent<Rigidbody>();

        animator = GetComponentInParent<Animator>();
        freeLookCamera = GameObject.FindWithTag("FreeLookCamera").GetComponent<Transform>();
    }

    public override IEnumerator Execute()
    {
        cooldown = cooldownThisSkill;

        dashVFX.StartTrail();
        RuntimeManager.PlayOneShotAttached(skillInfo.activateSkillSound, gameObject);

        animator.applyRootMotion = false;
        rb.useGravity = false;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(freeLookCamera.forward.normalized * blinkSpeed, ForceMode.Impulse);

        yield return new WaitForSeconds(duration);

        animator.applyRootMotion = true;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
    }
}
