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
    private bool isChoosingChallenge = false;

    private void Awake()
    {
        if(roundManager == null)
            roundManager = GameManager.Instance.roundManager;

        challengeCanvas.SetActive(false);
    }

    private void Update()
    {
        //Para que puedan escoger felizmente uwu
        if(isChoosingChallenge){
            Time.timeScale = 0f; // Pausa
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


    public void ShowChallenges()
    {
        // Clear any existing panels
        ClearChallengePanels();

        // Filtrado de los challenges dependiendo de la dificultad
        var filteredChallenges = allChallenges.FindAll(challenge =>
        {
            if (roundManager.CurrentRound <= 2) return challenge.difficulty == ChallengeDifficulty.Easy;
            if (roundManager.CurrentRound <= 3) return challenge.difficulty == ChallengeDifficulty.Medium;
            return challenge.difficulty == ChallengeDifficulty.Hard;
        });

        // Shuffle the filtered challenges to ensure randomness
        var shuffledChallenges = new List<ChallengeScriptableObject>(filteredChallenges);
        for (int i = 0; i < shuffledChallenges.Count; i++)
        {
            int randomIndex = Random.Range(0, shuffledChallenges.Count);
            var temp = shuffledChallenges[i];
            shuffledChallenges[i] = shuffledChallenges[randomIndex];
            shuffledChallenges[randomIndex] = temp;
        }

        // Select up to 3 unique challenges
        for (int i = 0; i < Mathf.Min(3, shuffledChallenges.Count); i++)
        {
            var challenge = shuffledChallenges[i];
            var challengePanel = Instantiate(challengePanelPrefab, canvasGroupLayout);
            var updateChallengeUI = challengePanel.GetComponent<UpdateChallengeUI>();

            if (updateChallengeUI != null)
            {
                updateChallengeUI.challengeManager = this;
                updateChallengeUI.SetChallengeData(challenge); // Pass the challenge data to the panel
            }

            activePanels.Add(challengePanel);
        }

        // Para mostrar el canvas de los challenges
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isChoosingChallenge = true;
        challengeCanvas.SetActive(true);
    }

    public void ActivateChallenge(ChallengeScriptableObject challengeSO)
    {
        // Crea un challenge como un nuevo GameObject en la escena
        var challengeGO = new GameObject(challengeSO.challengeName);
        challengeGO.transform.SetParent(this.transform);

        // Le añade el componente del challenge escogido
        var challenge = challengeGO.AddComponent(GetChallengeType(challengeSO.challengeType)) as Challenges;
        challenge.challengeManager = this;

        // Pone los valores del ScriptableObject al challenge
        challengeSO.SetUpChallenge(challenge);

        // Añade los challenges activos a una lista (De momento no hace nada mas)
        activeChallenges.Add(challenge);

        //Despues de escoger
        Time.timeScale = 1f; // Resume the game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isChoosingChallenge = false;
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
