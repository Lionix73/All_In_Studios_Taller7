using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private RoundManager roundManager;

    [SerializeField] private float score;
    [SerializeField] private float killedEnemies;
    [SerializeField] private TextMeshProUGUI _UIscoreText;

    public float KilledEnemies { get; set; }

    private void Start() {
        roundManager = GetComponent<RoundManager>();
        score = 0;
        killedEnemies = 0;
    }

    public void SetScore(float sco){
        score += sco;
        if(UIManager.Singleton) UIManager.Singleton.GetPlayerActualScore(score);
        if (_UIscoreText==null) return;
        _UIscoreText.text = $"{score}";

    }

    public float GetScore(){
        return score;
    }

    public void AddEnemyKilled(int killedEnemy)
    {
        killedEnemies++;
        if(UIManager.Singleton) UIManager.Singleton.GetPlayerActualKills(killedEnemies);
    }
}
