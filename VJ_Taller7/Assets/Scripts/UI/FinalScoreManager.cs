using System;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards.Models;
using Unity.Services.Leaderboards;
using UnityEngine;

public class FinalScoreManager : MonoBehaviour
{
    public static FinalScoreManager Singleton;

    [SerializeField] private GameObject leaderboardDeadParent;
    [SerializeField] private Transform leaderboardDeadContentParent;
    [SerializeField] private GameObject leaderboardWinParent;
    [SerializeField] private Transform leaderboardWinContentParent;
    [SerializeField] private Transform leaderboardItemPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }


    }

    public  void UpdateDiedLeaderboard(string name,int score, float kills)
    {
        LocalLeaderboardManager.Singleton.AddScore(name, score);

        foreach (Transform t in leaderboardDeadContentParent)
        {
            Destroy(t.gameObject);
        }
        Transform leaderboardItem = Instantiate(leaderboardItemPrefab, leaderboardDeadContentParent);
        leaderboardItem.GetChild(0).GetComponent<TextMeshProUGUI>().text = name;
        leaderboardItem.GetChild(1).GetComponent<TextMeshProUGUI>().text = score.ToString();
        leaderboardItem.GetChild(2).GetComponent<TextMeshProUGUI>().text = kills.ToString();
    }

    public  void UpdateWinLeaderboard(string name, int score, float kills)
    {
        LocalLeaderboardManager.Singleton.AddScore(name, score);

        foreach (Transform t in leaderboardWinContentParent)
        {
            Destroy(t.gameObject);
        }

        Transform leaderboardItem = Instantiate(leaderboardItemPrefab, leaderboardWinContentParent);

        leaderboardItem.GetChild(0).GetComponent<TextMeshProUGUI>().text = name;
        leaderboardItem.GetChild(1).GetComponent<TextMeshProUGUI>().text = score.ToString();
        leaderboardItem.GetChild(2).GetComponent<TextMeshProUGUI>().text = kills.ToString();
    }
}
