using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Challenge ScriptableObject", menuName = "Challenges/Challenge ScriptableObject")]
public class ChallengeScriptableObject : ScriptableObject
{
    [Header("Challenge Configuration")]
    public string challengeName;
    [TextArea] public string description;

    [Space(20)]
    public ChallengeType challengeType;
    public ChallengeDifficulty difficulty;
    public int xpReward;
    public RawImage icon;


    [Header("Conditional Challenges")]

    [Tooltip("The amount of enemies, weapons, seconds, etc. that the player needs to kill, collect, etc. If 0 no condition is set.")]
    public int requiredAmount;

    [Tooltip("If true, the player will not be able to heal during the challenge. If false, the player will be able to heal.")]
    public bool noHeal; 

    public void SetUpChallenge(Challenges challenge)
    {
        challenge.ChallengeName = challengeName;
        challenge.Description = description;
        challenge.ChallengeType = challengeType;
        challenge.Difficulty = difficulty;
        challenge.XpReward = xpReward;
        challenge.Icon = icon;

        challenge.RequiredAmount = requiredAmount;
        challenge.NoHeal = noHeal;
    }
}
