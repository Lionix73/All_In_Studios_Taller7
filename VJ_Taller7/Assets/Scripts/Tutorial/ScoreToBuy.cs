using UnityEngine;

public class ScoreToBuy : MonoBehaviour
{
    [Tooltip("Score given to the player")]
    [SerializeField] private float score;

    public void GiveScore()
    {
        GameManager.Instance.scoreManager.SetScore(score);
    }
}
