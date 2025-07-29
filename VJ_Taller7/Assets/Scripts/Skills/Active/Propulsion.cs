using System.Collections;
using FMODUnity;
using UnityEngine;

public class Propulsion : SkillBase
{
    [SerializeField] private float duration = 3f;
    [SerializeField] private float cooldownThisSkill = 5f;
    [SerializeField][Range(0, 50)] private float force = 20f;
    [SerializeField] private ParticleSystem vfx;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    public override IEnumerator Execute()
    {
        cooldown = cooldownThisSkill;

        vfx.Play();
        RuntimeManager.PlayOneShotAttached(skillInfo.activateSkillSound, gameObject);

        rb.linearVelocity = Vector3.zero;

        float timer = 0;
        while (timer < duration)
        {
            rb.AddForce(Vector3.up * force, ForceMode.Acceleration);

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
