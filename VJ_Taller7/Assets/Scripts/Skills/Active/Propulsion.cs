using System.Collections;
using UnityEngine;

public class Propulsion : SkillBase
{
    [SerializeField] private float duration = 3f;
    [SerializeField] private float cooldownThisSkill = 5f;
    [SerializeField] private float force = 10f;
    
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        cooldown = cooldownThisSkill;
    }

    public override IEnumerator Execute()
    {
        float timer = 0;

        while (timer < duration)
        {
            rb.AddForce(Vector3.up * force, ForceMode.Force);

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
