using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [SerializeField] private SkillBase[] skills;
    [SerializeField] private int activeSkillIndex;

    public void ActivateSkill()
    {
        if (activeSkillIndex >= 0 && activeSkillIndex < skills.Length)
        {
            skills[activeSkillIndex].Activate();
        }
        else
        {
            Debug.LogWarning("Índice de habilidad fuera de rango.");
        }
    }
}
