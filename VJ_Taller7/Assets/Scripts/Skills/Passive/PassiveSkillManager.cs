using UnityEngine;

public class PassiveSkillManager : MonoBehaviour
{
    [SerializeField] private PassiveSkillBase[] passiveSkills;

    private void Update()
    {
        foreach (var skill in passiveSkills)
        {
            if (!skill.IsOnCooldown)
                skill.CheckCondition();
        }
    }
}
