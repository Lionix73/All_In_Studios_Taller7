using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class LocalLeaderboardManager : MonoBehaviour
{
    public static LocalLeaderboardManager Singleton;

    [SerializeField] private Transform leaderboardContentParent;
    [SerializeField] private Transform leaderboardItemPrefab;

    private const string LeaderboardKey = "LocalLeaderboard_EggDivers";
    private List<LeaderboardEntry> localEntries = new List<LeaderboardEntry>();

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(gameObject); // Persistir entre escenas
            LoadLeaderboard(); // Cargar datos al iniciar
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Guarda una nueva puntuación cuando el jugador termina el juego
    public void AddScore(string playerName, int score)
    {
        string playerId = System.Guid.NewGuid().ToString(); // Nuevo ID único por partida
        localEntries.Add(new LeaderboardEntry(playerId, playerName, score));

        // Ordenar de mayor a menor puntuación y guardar
        localEntries = localEntries.OrderByDescending(e => e.Score).ToList();
        SaveLeaderboard();
    }

    // Actualiza la UI del leaderboard
    public void UpdateLeaderboardUI()
    {
        // Limpiar leaderboard anterior
        foreach (Transform child in leaderboardContentParent)
        {
            Destroy(child.gameObject);
        }

        // Mostrar las top 10 puntuaciones
        int maxEntries = Mathf.Min(localEntries.Count, 20);
        for (int i = 0; i < maxEntries; i++)
        {
            var entry = localEntries[i];
            Transform item = Instantiate(leaderboardItemPrefab, leaderboardContentParent);
            item.GetChild(0).GetComponent<TextMeshProUGUI>().text = entry.PlayerName;
            item.GetChild(1).GetComponent<TextMeshProUGUI>().text = entry.Score.ToString();
        }
    }

    // ====== Sistema de Guardado ======
    private void SaveLeaderboard()
    {
        string json = JsonUtility.ToJson(new LeaderboardDataWrapper(localEntries));
        PlayerPrefs.SetString(LeaderboardKey, json);
        PlayerPrefs.Save();
    }

    private void LoadLeaderboard()
    {
        if (PlayerPrefs.HasKey(LeaderboardKey))
        {
            string json = PlayerPrefs.GetString(LeaderboardKey);
            LeaderboardDataWrapper wrapper = JsonUtility.FromJson<LeaderboardDataWrapper>(json);
            localEntries = wrapper.Entries;
        }
    }

    // Clases auxiliares para el guardado
    [System.Serializable]
    private class LeaderboardEntry
    {
        public string PlayerId;
        public string PlayerName;
        public int Score;

        public LeaderboardEntry(string id, string name, int score)
        {
            PlayerId = id;
            PlayerName = name;
            Score = score;
        }
    }

    [System.Serializable]
    private class LeaderboardDataWrapper
    {
        public List<LeaderboardEntry> Entries;

        public LeaderboardDataWrapper(List<LeaderboardEntry> entries)
        {
            Entries = entries;
        }
    }
}