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
            // 1?? **Inicializa UnityServices primero**
            await UnityServices.InitializeAsync();

            // 2?? **Cierra sesión si ya existe una activa**
            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut();
                ClearPlayerData();
            }

            // 3?? **Autenticación anónima (genera nuevo ID)**
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Player ID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error en inicialización: {e.Message}");
        }
    }

    private void ClearPlayerData()
    {
        PlayerPrefs.DeleteKey("UnityServicesToken");
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
