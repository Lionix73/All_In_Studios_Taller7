using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateChallengeUI : MonoBehaviour
{
    [SerializeField] private RawImage iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [HideInInspector] public ChallengeManager challengeManager;
    private ChallengeScriptableObject challengeSO; // Associated ChallengeScriptableObject

    public void SetChallengeData(ChallengeScriptableObject challengeSO)
    {
        this.challengeSO = challengeSO;

        // Update the UI elements
        titleText.text = challengeSO.challengeName;
        descriptionText.text = challengeSO.description;
        //iconImage.texture = challengeSO.icon.texture;
    }

    public void OnClick(){
        Debug.Log("Clicked on challenge: " + challengeSO.challengeName);
        challengeManager.ActivateChallenge(challengeSO);
    }
}
