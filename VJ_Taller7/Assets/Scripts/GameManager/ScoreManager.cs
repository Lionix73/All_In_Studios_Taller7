using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public RoundManager roundManager;

    [SerializeField] private float score;
    [SerializeField] private TextMeshProUGUI _UIscoreText;

    private void Start() {
        roundManager = GetComponent<RoundManager>();
        score = 0;
    }

    public void SetScore(int sco){
        score += sco;
        if (_UIscoreText==null) return;
        _UIscoreText.text = $"{score}";
    }
}
