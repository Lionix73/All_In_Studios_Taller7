using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    public static UIManager Singleton
    {
        get => _singleton;

        set
        {
            if (value == null)
                _singleton = null;
            else if (_singleton == null)
            { 
                _singleton = value;
                DontDestroyOnLoad(value);
            }
            else if (_singleton != value)
            {
                Destroy(value.gameObject);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                Debug.LogError($"There should only ever be one instance of {nameof(UIManager)}");
            }
        }
    }

    public static UIManager _singleton;

    public void Awake()
    {
        Singleton = this;

        // Asegúrate de que ambas listas tengan la misma cantidad de elementos
        if (sceneList.Count != gameObjectsScenes.Count)
        {
            Debug.LogError("Las listas sceneList y gameObjectsScenes no tienen la misma cantidad de elementos!");
            return;
        }

        for (int i = 0; i < sceneList.Count; i++)
        {
            SceneScriptableObject sceneData = sceneList[i];
            GameObject sceneGO = gameObjectsScenes[i];

            SceneInfoButton button = sceneGO.GetComponent<SceneInfoButton>();

            if (button != null)
            {
                button.SetSceneData(sceneData);
            }
            else
            {
                Debug.LogWarning($"El GameObject {sceneGO.name} no tiene un componente SceneInfoButton");
            }
        }
        IsMainMenu = true;
    }

    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;

    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame(4);
        }

    }

    private float killedEnemiesUI = 0;
    [SerializeField] private bool hasWon = false;
    [SerializeField] private bool IsPaused = false;
    [SerializeField] private bool IsDead = false;
    [SerializeField] private bool IsMainMenu = false;
    public bool actualRoundDisplay = true;
    [SerializeField] GameObject[] screens;
    [SerializeField] int activeScene;
    [SerializeField] Image healthBar;

    [SerializeField] private List<SceneScriptableObject> sceneList = new List<SceneScriptableObject>();
    [SerializeField] private List<GameObject> gameObjectsScenes = new List<GameObject>();

    [SerializeField] private Image gunImage;
    [SerializeField] private TextMeshProUGUI gunTypeText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI maxTotalAmmoText;
    [SerializeField] private TextMeshProUGUI enemiesKilledText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI scoreInGameText;
    [SerializeField] private TextMeshProUGUI UiWaveTimer;
    [SerializeField] private TextMeshProUGUI UiBetweenWavesTimer;
    [SerializeField] private TextMeshProUGUI UiWaveCounter;
    [SerializeField] private TextMeshProUGUI UiRoundCounter;
    [SerializeField] private TextMeshProUGUI UiEnemyCounter;
    [SerializeField] private GameObject UIWaves;


    public void StartGameUI()
    {
        IsMainMenu = false;
        screens[5].SetActive(true);
        actualRoundDisplay = true;
        UiWaveTimer.text = "";
        UiWaveCounter.text = "";
        UiEnemyCounter.text = "";
        UiRoundCounter.gameObject.SetActive(true);
        characterNameText.text = CharacterManager.Instance.characters[CharacterManager.Instance.selectedIndexCharacter].name;
        UiRoundCounter.text = "";
        scoreInGameText.text = "Score: ";

    }
    public void SelectedScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
    public void Show(int indexScreen)
    {
       activeScene = indexScreen;
       screens[0].SetActive(false);
       screens[activeScene].SetActive(true);    
    }
    public void Hide()
    {
        screens[0].SetActive(true);
        screens[activeScene].SetActive(false);
    }
    public void PauseGame(int indexPauseScreen)
    {
        if (IsDead) return;
        if (hasWon) return;
        if (IsMainMenu) return;
        IsPaused = !IsPaused;
        Cursor.visible = IsPaused;
        screens[indexPauseScreen].SetActive(IsPaused);
        if(IsPaused)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

    }
    public void DiedUI(int indexDiedUI)
    {
        if (IsPaused)
        {
            PauseGame(4);
        }

        IsDead = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        screens[indexDiedUI].SetActive(true);
        screens[5].SetActive(false);
    }
    public void WinUI(int indexWinUI)
    {
        if (IsPaused)
        {
            PauseGame(4);
        }

        hasWon = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        screens[indexWinUI].SetActive(true);
        screens[5].SetActive(false);
    }
    public void SetCameraCanva()
    {
        screens[2].GetComponent<Canvas>().worldCamera = Camera.main;
    }
    public void PlayScene(int indexInGameUI)
    {
        screens[0].SetActive(false);
        screens[indexInGameUI].SetActive(true);
    }
    public void BackToMenu(int indexInGameUI)
    {
        IsMainMenu = true;
        screens[0].SetActive(true);
        screens[indexInGameUI].SetActive(false);
        IsDead = false;
        hasWon = false;

    }

    public void GetPlayerHealth(float playerHealth, float maxHealth)
    {
        healthBar.fillAmount = Mathf.Clamp(playerHealth / maxHealth, 0, 1);
    }
    public void RestartGame()
    {
        StartGameUI();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        IsDead = false;
    }
    public void GetPlayerGunInfo(int actualAmmo,int maxActualAmmo, GunScriptableObject equippedGun)
    {
        GetPlayerActualAmmo(actualAmmo, maxActualAmmo);
        gunTypeText.text = $"{equippedGun.name}";
        gunImage.sprite = equippedGun.UIImage;
    }
    public void GetPlayerActualAmmo(int actualAmmo, int maxActualAmmo)
    {
        ammoText.text = $"{actualAmmo} / {maxActualAmmo}";
    }
    public void GetPlayerTotalAmmo(int totalAmmo)
    {
        maxTotalAmmoText.text = $"{totalAmmo}";
    }

    public void GetPlayerActualKills(float actualKills)
    {
        enemiesKilledText.text = actualKills.ToString();
    }

    public void GetPlayerActualScore(float actualScore)
    {
        scoreText.text = actualScore.ToString();
        scoreInGameText.text = $"Score: {actualScore}";
    }

    public void UIChangeRound(int currentRound)
    {
        if (!actualRoundDisplay) return;

        UiWaveTimer.text = "";
        UiWaveCounter.text = "";
        UiEnemyCounter.text = "";
        Dialogue roundDialogue = UiRoundCounter.GetComponent<Dialogue>();
        roundDialogue.Lines[0] = $"RONDA {currentRound}";
        roundDialogue.Delays[0] = (roundDialogue.TextSpeed * 9);
        roundDialogue.StartDialogue();
        actualRoundDisplay = false;

    }

    public void UIBetweenWavesTimer(float inBetweenRoundsTimer)
    {
        Dialogue roundDialogue = UiRoundCounter.GetComponent<Dialogue>();
        if (roundDialogue.FinishDialogue)
        {
            UiBetweenWavesTimer.text = $"Siguiente ronda en: \n {Mathf.FloorToInt(inBetweenRoundsTimer / 60)} : {Mathf.FloorToInt(inBetweenRoundsTimer % 60)}";
        }
    }
    public void UIBetweenWaves(float waveTimer)
    {
        UiWaveTimer.text = $"Tiempo restante: \n {Mathf.FloorToInt(waveTimer / 60)} : {Mathf.FloorToInt(waveTimer % 60)}";
    }

    public void UIActualWave(int currentWave)
    {
        UiWaveCounter.text = $"Oleada: {currentWave} /3";
    }

    public void UIEnemiesAlive(int enemiesAlive)
    {
        UiEnemyCounter.text = $"Enemigos Restantes: {enemiesAlive}";
    }

}
