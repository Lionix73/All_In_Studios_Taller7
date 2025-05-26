using UnityEngine;
using TMPro;
using System.Xml.Schema;

public class ScoreManager : MonoBehaviour
{
    private RoundManager roundManager;

    [SerializeField] private float score;
    [SerializeField] private float killedEnemies;
    [SerializeField] private TextMeshProUGUI _UIscoreText;
    private float totalScore;
    public float KilledEnemies { get; set; }

    private void Start() {
        roundManager = GetComponent<RoundManager>();
        score = 0;
        killedEnemies = 0;
    }

    public void SetScore(float sco){
        score += sco;
        totalScore += sco;
        GameManager.Instance.ScoreChange(score);
        if(UIManager.Singleton) UIManager.Singleton.GetPlayerActualScore(score);
        if (_UIscoreText==null) return;
        _UIscoreText.text = $"{score}";

    }

    public float GetScore(){
        return score;
    }
    public float GetKilledEnemies() 
    {
        return killedEnemies;
    }

    public float GetTotalScore()
    {
        return totalScore;
    }
    public void AddEnemyKilled(int killedEnemy)
    {
        killedEnemies++;
    }
}
