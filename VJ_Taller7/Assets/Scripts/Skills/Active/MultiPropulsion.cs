using System.Collections;
using UnityEngine;

public class MultiPropulsion : MultiSkillBase
{
    [SerializeField] private float duration = 3f;
    [SerializeField] private float cooldownThisSkill = 5f;
    [SerializeField][Range(0,25)] private float force = 20f;
    
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    public override IEnumerator Execute()
    {
        cooldown = cooldownThisSkill;

        float timer = 0;
        while (timer < duration)
        {
            rb.AddForce(Vector3.up * force, ForceMode.Acceleration);

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
