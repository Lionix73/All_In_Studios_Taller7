using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Info", menuName = "Scriptable Objects/Skill_Info")]
public class Skill_Info : ScriptableObject
{

    [Header("Skill Index")]
    public int indexSkill;

    [Header("Skill Icon")]
    public Sprite image;

    [Header("Skill Title")]
    public string title;

    [Header("Skill Description")]
    public string[] descriptionItems;
    public string[] descriptionText;
}
