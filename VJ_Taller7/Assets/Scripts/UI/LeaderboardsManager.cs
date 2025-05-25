using UnityEngine;
using Unity.Services.Core;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using Unity.Services.Authentication;
using System;
using Unity.Services.Leaderboards.Models;
using TMPro;
public class LeaderboardsManager : MonoBehaviour
{
    public static LeaderboardsManager Singleton;

    [SerializeField] private GameObject leaderboardParent;
    [SerializeField] private Transform leaderboardContentParent;
    [SerializeField] private Transform leaderboardItemPrefab;

    private string leaderboardID = "Leaderboard_EggDivers";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }


    }
    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

    }

    public async void UpdateLeaderboard()
    {
            LeaderboardScoresPage leaderboardScoresPage = await LeaderboardsService.Instance.GetScoresAsync(leaderboardID);

            foreach (Transform t in leaderboardContentParent)
            {
                Destroy(t.gameObject);
            }

            foreach (LeaderboardEntry entry in leaderboardScoresPage.Results)
            {
                Transform leaderboardItem = Instantiate(leaderboardItemPrefab, leaderboardContentParent);
                leaderboardItem.GetChild(0).GetComponent<TextMeshProUGUI>().text = entry.PlayerName;
                leaderboardItem.GetChild(1).GetComponent<TextMeshProUGUI>().text = entry.Score.ToString();

            }
    }

    public string GetLeaderboardID()
    { return leaderboardID; }
}
