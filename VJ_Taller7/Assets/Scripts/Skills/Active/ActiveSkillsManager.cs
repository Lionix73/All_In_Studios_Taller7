using UnityEngine;

public class ActiveSkillManager : SkillsManagerBase
{
    public SkillBase[] skills;
    public int activeSkillIndex;

    private void OnValidate()
    {
        activeSkillIndex = Mathf.Clamp(activeSkillIndex, 0, skills.Length);
    }

    void Awake()
    {
        ActivateSkillGameObject();
    }

    private void Start()
    {
        if (CharacterManager.Instance != null)
            activeSkillIndex = CharacterManager.Instance.indexActiveSkill;

        SearchSkillsUI();
        SetupSkillsIcons();
    }

    private void SetupSkillsIcons()
    {
        Sprite skillSprite = skills[activeSkillIndex].skillInfo.image;
        SetupShaderMaterial(_actSkillImg, skillSprite);
    }

    private void ActivateSkillGameObject()
    {
        skills[activeSkillIndex].gameObject.SetActive(true);
    }

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
