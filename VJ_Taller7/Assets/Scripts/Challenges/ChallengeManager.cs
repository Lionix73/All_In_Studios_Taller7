using System.Collections.Generic;
using UnityEngine;

public class ChallengeManager : MonoBehaviour
{
    [SerializeField] private List<ChallengeScriptableObject> allChallenges;
    [SerializeField] private GameObject challengeCanvas;
    [SerializeField] private GameObject challengePanelPrefab;
    [SerializeField] private Transform canvasGroupLayout;
    [SerializeField] private RoundManager roundManager;

    private List<Challenges> activeChallenges = new List<Challenges>();
    private List<GameObject> activePanels = new List<GameObject>();


    private void Awake()
    {
        if(roundManager == null)
            roundManager = GameManager.Instance.roundManager;

        challengeCanvas.SetActive(false);
    }

    private void Start()
    {
        // Subscribe to the round complete event
        roundManager.OnRoundComplete += HandleRoundComplete;
    }

    private void HandleRoundComplete(){
        ShowChallenges();
    }

public void ShowChallenges()
{
    // Clear any existing panels
    ClearChallengePanels();

    for (int i = 0; i < 3; i++)
    {
        var randomChallenge = allChallenges[Random.Range(0, allChallenges.Count)];
        var challengePanel = Instantiate(challengePanelPrefab, canvasGroupLayout);
        var updateChallengeUI = challengePanel.GetComponent<UpdateChallengeUI>();

        // Associate the ChallengeScriptableObject with the panel
        if (updateChallengeUI != null)
        {
            updateChallengeUI.challengeManager = this;
            updateChallengeUI.SetChallengeData(randomChallenge); // Pass the challenge data to the panel
        }

        // Add the panel to the activePanels list
        activePanels.Add(challengePanel);
    }

    challengeCanvas.SetActive(true);
}

    public void ActivateChallenge(ChallengeScriptableObject challengeSO)
    {
        // Create a new GameObject for the active challenge
        var challengeGO = new GameObject("ActiveChallenge");
        challengeGO.transform.SetParent(this.transform);

        // Add the appropriate challenge script based on the ChallengeType
        var challenge = challengeGO.AddComponent(GetChallengeType(challengeSO.challengeType)) as Challenges;
        challenge.challengeManager = this;

        // Set up the challenge using the ScriptableObject
        challengeSO.SetUpChallenge(challenge);

        // Add the challenge to the active challenges list
        activeChallenges.Add(challenge);

        challengeCanvas.SetActive(false);
    }

    private System.Type GetChallengeType(ChallengeType challengeType)
    {
        return challengeType switch
        {
            ChallengeType.KillEnemies => typeof(KillChallenge),
            ChallengeType.HealthRelated => typeof(HealthChallenge),
            ChallengeType.AbilityRelated => typeof(AbilityChallenge),
            _ => throw new System.ArgumentOutOfRangeException(nameof(challengeType), challengeType, null)
        };
    }

    public void CompleteChallenge(Challenges completedChallenge){
        //Give Player XP
        //Upgrade Player weapon Stats
        activeChallenges.Remove(completedChallenge);
        Destroy(completedChallenge.gameObject);
    }

    private void ClearChallengePanels()
    {
        // Destroy all active panels
        foreach (var panel in activePanels)
        {
            Destroy(panel);
        }

        activePanels.Clear();
    }
}
