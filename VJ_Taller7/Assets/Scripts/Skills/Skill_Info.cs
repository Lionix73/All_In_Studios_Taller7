using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Info", menuName = "Scriptable Objects/Skill_Info")]
public class Skill_Info : ScriptableObject
{

    [Header("Skill Index")]
    public int indexSkill;

    [Header("Skill Icon")]
    public Sprite image;
    public Sprite backgroundImage;

    [Header("Skill Title")]
    public string title;
    public Sprite titleImage;

    [Header("Skill Description")]
    public string[] descriptionItems;
    public string[] descriptionText;

    [Header("SkillMode")]
    public bool isMultiplayer;
}
