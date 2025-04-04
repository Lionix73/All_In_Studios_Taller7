using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [SerializeField] private SkillBase[] skills;
    [SerializeField] private int activeSkillIndex;

    private void Start()
    {
        ActivateSkill(activeSkillIndex);    
    }

    public void ActivateSkill(int index)
    {
        if (index >= 0 && index < skills.Length)
        {
            skills[index].Activate();
        }
        else
        {
            Debug.LogWarning("Índice de habilidad fuera de rango.");
        }
    }
}
