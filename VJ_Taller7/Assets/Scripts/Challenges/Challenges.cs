using UnityEngine;
using UnityEngine.UI;

public abstract class Challenges : MonoBehaviour
{
    [Header("Challenge Configuration")]
    [SerializeField] protected string challengeName;
    [SerializeField] [TextArea] protected string description;
    [SerializeField] protected ChallengeType challengeType;
    [SerializeField] protected ChallengeDifficulty difficulty;
    [SerializeField] protected int xpReward;
    [SerializeField] protected RawImage icon;

    public string ChallengeName {get => challengeName; set => challengeName = value;}
    public string Description {get => description; set => description = value;}
    public ChallengeType ChallengeType {get => challengeType; set => challengeType = value;}
    public ChallengeDifficulty Difficulty {get => difficulty; set => difficulty = value;}
    public int XpReward {get => xpReward; set => xpReward = value;}
    public RawImage Icon {get => icon; set => icon = value;}


    [Header("Conditional Challenges")]

    [Tooltip("The amount of enemies, weapons, seconds, etc. that the player needs to kill, collect, etc. If 0 no condition is set.")]
    [SerializeField] protected int requiredAmount;

    [Tooltip("If true, the player will not be able to heal during the challenge. If false, the player will be able to heal.")]
    [SerializeField] protected bool noHeal; 

    public int RequiredAmount {get => requiredAmount; set => requiredAmount = value;}
    public bool NoHeal {get => noHeal; set => noHeal = value;}


    // Progression variables
    protected int currentProgress;
    protected bool isCompleted;
    [HideInInspector] public ChallengeManager challengeManager;

    protected void Awake()
    {
        if(challengeManager == null)
        challengeManager = FindFirstObjectByType<ChallengeManager>();
        currentProgress = 0;
        isCompleted = false;
    }

    protected void CheckCompletition(){
        if(currentProgress >= requiredAmount && !isCompleted){
            CompleteChallenge();
        }
    }

    protected virtual void CompleteChallenge(){
        isCompleted = true;
        UnsuscribeEvents();
        challengeManager.CompleteChallenge(this);
    }

    protected abstract void SuscribeEvents();
    protected abstract void UnsuscribeEvents();
}
