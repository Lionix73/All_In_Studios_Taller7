using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
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
        playerNameProfile = "Sigma";

        // Aseg�rate de que ambas listas tengan la misma cantidad de elementos
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

        soundManager = GetComponentInChildren<ThisObjectSounds>();
        defaultColor = scoreInGameText.color;



    }
    private void Start()
    {
        // Inicializar el diccionario de paneles
        foreach (var panel in uiPanels)
        {
            panelDictionary.Add(panel.gameObject.name, panel);
            panel.gameObject.SetActive(false); // Asegurarse que todos están desactivados al inicio
        }
        ShowPanel(mainMenuPannel);
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
            if (!IsPaused)
                soundManager.PlaySound("PausedButton");
            else
                soundManager.PlaySound("PlayButton");

            PauseGame(4);
        }
    }

    private float killedEnemiesUI = 0;
    [SerializeField] private float waitingTimeForMenu = 0;
    [SerializeField] private bool hasWon = false;
    public bool IsPaused { get; private set; } = false;
    public bool IsDead { get; private set; } = false;
    public bool IsMainMenu { get; private set; } = false;
    public bool actualRoundDisplay = true;
    [SerializeField] Image healthBar;

    [SerializeField] private List<SceneScriptableObject> sceneList = new List<SceneScriptableObject>();
    [SerializeField] private List<GameObject> gameObjectsScenes = new List<GameObject>();

    [SerializeField] private TMP_InputField inputFieldName;

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
    [SerializeField] private TextMeshProUGUI UiInstructionToPassRound;
    [SerializeField] private List<GameObject> playerNameText = new List<GameObject>();
    [SerializeField] private GameObject UIWaves;

    private string playerNameProfile;

    private ThisObjectSounds soundManager;

    public static UIManager Instance;
    [SerializeField] private float fadeTime = 1.0f;
    [SerializeField] private AnimationList[] uiPanels; // Todos los paneles que quieres controlar
    [SerializeField] private string mainMenuPannel;

    private float currentDisplayedScore;
    // Colores para los estados
    public Color increasingColor = Color.green; // Color cuando el puntaje aumenta
    public Color decreasingColor = Color.red;   // Color cuando el puntaje disminuye
    private Color defaultColor;

    [Header("Round Number UI (Units Only)")]
    [SerializeField] private List <Image> unitDigitImageRound;  // Imagen donde se mostrará el dígito (0-9)
    [SerializeField] private List<Sprite> digitSpritesRound;  // Sprites de los números 0 al 9 (en orden)
    [Header("Round Number UI (Units Only)")]
    [SerializeField] private List<Image> unitDigitImageWave;  // Imagen donde se mostrará el dígito (0-9)
    [SerializeField] private List<Sprite> digitSpritesWave;  // Sprites de los números 0 al 9 (en orden)

    private Dictionary<string, AnimationList> panelDictionary = new Dictionary<string, AnimationList>();

    // Función pública para mostrar un panel con fade in
    public void ShowPanel(string panelName)
    {
        if (panelDictionary.TryGetValue(panelName, out AnimationList panel))
        {
            panel.PanelFadeIn(fadeTime);
        }
        else
        {
            Debug.LogWarning($"Panel '{panelName}' no encontrado en UIManager");
        }
    }

    public void ShowPartialPanel(string panelName, float delayBetwwenPanel)
    {
        if (panelDictionary.TryGetValue(panelName, out AnimationList panel))
        {
            panel.ShowPanelAndHide(delayBetwwenPanel);
        }
        else
        {
            Debug.LogWarning($"Panel '{panelName}' no encontrado en UIManager");
        }
    }

    // Función pública para ocultar un panel con fade out
    public void HidePanel(string panelName)
    {
        if (panelDictionary.TryGetValue(panelName, out AnimationList panel))
        {
            if (panel.isActiveAndEnabled)
                panel.PanelFadeOut(fadeTime);
        }
        else
        {
            Debug.LogWarning($"Panel '{panelName}' no encontrado en UIManager");
        }
    }

    // Función para alternar un panel (si está visible lo oculta, si está oculto lo muestra)
    public void TogglePanel(string panelName)
    {
        if (panelDictionary.TryGetValue(panelName, out AnimationList panel))
        {
            if (panel.gameObject.activeSelf)
            {
                HidePanel(panelName);
            }
            else
            {
                ShowPanel(panelName);
            }
        }
    }

    // Formato: "panelToHide|panelToShow"
    public void SwitchPanels(string panels)
    {
        string[] parts = panels.Split('/');
        if (parts.Length == 2)
        {
            StartCoroutine(SwitchPanelsRoutine(parts[0], parts[1]));
        }
    }


    private IEnumerator SwitchPanelsRoutine(string panelToHide, string panelToShow)
    {
        if (panelDictionary.TryGetValue(panelToHide, out AnimationList hidePanel))
        {
            // Esperar un poco antes de mostrar el nuevo panel
            yield return null;

            if (hidePanel.isActiveAndEnabled)
                StartCoroutine(hidePanel.PanelFadeOutRoutine(fadeTime));
        }



        if (panelDictionary.TryGetValue(panelToShow, out AnimationList showPanel))
        {
            showPanel.PanelFadeIn(fadeTime);
        }
    }

    public void StartGameUI()
    {
        if (panelDictionary.TryGetValue("SkillsMenu", out AnimationList hidePanel))
        {
            if (hidePanel.isActiveAndEnabled)
                hidePanel.gameObject.SetActive(false);
        }
        if (panelDictionary.TryGetValue(mainMenuPannel, out AnimationList hideMenuPanel))
        {
            if (hideMenuPanel.isActiveAndEnabled)
                hideMenuPanel.gameObject.SetActive(false);
        }

        IsMainMenu = false;
        ShowPanel("UIPlayer");
        actualRoundDisplay = true;
        UiWaveTimer.text = "";
        UiWaveCounter.text = "";
        UiEnemyCounter.text = "";
        UiRoundCounter.gameObject.SetActive(true);
        characterNameText.text = CharacterManager.Instance.characters[CharacterManager.Instance.selectedIndexCharacter].name;
        UiRoundCounter.text = "";
        scoreInGameText.text = "00";

    }
    public void SelectedScene(string scene)
    {
        SceneTransitionManager.LoadScene(scene);
    }
    public void PauseGame(int indexPauseScreen)
    {
        if (IsDead) return;
        if (hasWon) return;
        if (IsMainMenu) return;
        if (panelDictionary.TryGetValue("Settings", out AnimationList settingsPannel))
        {
            if (settingsPannel.isActiveAndEnabled) return;
        }

        if (GameManager.Instance!=null) GameManager.Instance.PauseGame();
        
        IsPaused = !IsPaused;
        Cursor.visible = IsPaused;
        uiPanels[indexPauseScreen].gameObject.SetActive(IsPaused);
        if(IsPaused)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

    }
    public void FinalUI(bool HasWon)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (IsPaused)
        {
            PauseGame(4);
        }

        if (HasWon)
        {
            hasWon = true;
            SwitchPanels("UIPlayer/WinScene");
            StartCoroutine(WaitingTimeForMainMenu("WinScene/MainMenu"));
        }
        else
        {
            IsDead = true;
            SwitchPanels("UIPlayer/DeathScene");
            StartCoroutine(WaitingTimeForMainMenu("DeathScene/MainMenu"));

        }
    }

    public IEnumerator WaitingTimeForMainMenu(string UISwitchingPanels)
    {
        yield return new WaitForSeconds(waitingTimeForMenu);

        if(IsDead)
        {
            FindFirstObjectByType<OmnipotentSoundManager>().StopEverySound();
        }
        SwitchPanels(UISwitchingPanels);
        BackToMenu();

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
        SwitchPanels("UIPlayer/DeathScene");
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
        SwitchPanels("UIPlayer/WinScene");
    }

    public void BackToMenu()
    {
        IsMainMenu = true;
        //uiPanels[indexInGameUI].GetComponent<Canvas>().worldCamera = Camera.main;
        IsDead = false;
        hasWon = false;
        SceneTransitionManager.instance.LoadSceneWithLoadingScreen("MainMenu");

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
        ammoText.text = $"{actualAmmo}/{maxActualAmmo}";
    }
    public void GetPlayerTotalAmmo(int totalAmmo)
    {
        maxTotalAmmoText.text = $"{totalAmmo}";
    }

    public void GetPlayerActualKills(float actualKills)
    {
        enemiesKilledText.text = actualKills.ToString();
    }

    // Método público para actualizar el puntaje
    public void GetPlayerActualScore(float actualScore)
    {

        // Determinamos si el puntaje está aumentando o disminuyendo
        bool isIncreasing = actualScore > currentDisplayedScore;

        // Iniciamos una nueva corrutina para animar el puntaje
        StartCoroutine(AnimateScoreChange(currentDisplayedScore, actualScore, isIncreasing));
    }

    // Corrutina para animar el cambio de puntaje
    private IEnumerator AnimateScoreChange(float fromScore, float toScore, bool isIncreasing)
    {
        float duration = 0.75f; // Duración de la animación en segundos
        float elapsedTime = 0f;

        // Aplicamos el color correspondiente
        Color targetColor = isIncreasing ? increasingColor : decreasingColor;
        scoreInGameText.color = targetColor;

        while (elapsedTime < duration)
        {
            // Interpolamos linealmente entre el puntaje anterior y el nuevo
            currentDisplayedScore = Mathf.Lerp(fromScore, toScore, elapsedTime / duration);

            // Actualizamos los textos
            UpdateScoreTexts(currentDisplayedScore);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Aseguramos que al final muestre el valor exacto
        currentDisplayedScore = toScore;
        UpdateScoreTexts(currentDisplayedScore);

        // Restauramos el color original después de un pequeño delay (opcional)
        yield return new WaitForSeconds(0.2f);
        scoreInGameText.color = defaultColor;
    }

    private void UpdateScoreTexts(float score)
    {
        scoreText.text = $"Score:{Mathf.RoundToInt(score)}";
        scoreInGameText.text = $"{Mathf.RoundToInt(score)}";
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

    public void UIChangeImageRound(int currentRound)
    {
        Debug.Log(currentRound);
        //if (!actualRoundDisplay) return;

        // Limpiar otros textos si es necesario
        UiWaveTimer.text = "";
        UiWaveCounter.text = "";
        UiEnemyCounter.text = "";

        // --- Actualizar el dígito de unidades ---
        UpdateUnitDigitRound(currentRound -1);
        //actualRoundDisplay=false;

    }
    private void UpdateUnitDigitRound(int number)
    {
        // Asegurar que el número esté en el rango 0-9
        int unitDigit = Mathf.Clamp(number, 0, 9) % 10;  // % 10 por si acaso recibe un número > 9

        // Verificar que el sprite exista
        if (unitDigit >= 0 && unitDigit < digitSpritesRound.Count && digitSpritesRound[unitDigit] != null)
        {
            foreach (var image in unitDigitImageRound)
            {
                image.sprite = digitSpritesRound[unitDigit];
            }
            //unitDigitImageRound.gameObject.SetActive(true);  // Asegurarse de que esté visible
            ShowPartialPanel("RoundStartUI", 2);
        }
        else
        {
            Debug.LogWarning($"No hay un sprite asignado para el dígito {unitDigit}");
            //unitDigitImageRound.gameObject.SetActive(false);
        }
    }
    public void UIChangeImageWave(int currentWave)
    {
        Debug.Log(currentWave);
        //if (!actualRoundDisplay) return;

        // Limpiar otros textos si es necesario


        // --- Actualizar el dígito de unidades ---
        UpdateUnitDigitWave(currentWave - 1);
        //actualRoundDisplay=false;

    }
    private void UpdateUnitDigitWave(int number)
    {
        // Asegurar que el número esté en el rango 0-9
        int unitDigit = Mathf.Clamp(number, 0, 9) % 10;  // % 10 por si acaso recibe un número > 9

        // Verificar que el sprite exista
        if (unitDigit >= 0 && unitDigit < digitSpritesWave.Count && digitSpritesWave[unitDigit] != null)
        {
            foreach (var image in unitDigitImageWave)
            {
                image.sprite = digitSpritesWave[unitDigit];
            }
            //unitDigitImageRound.gameObject.SetActive(true);  // Asegurarse de que esté visible
            ShowPartialPanel("WaveStartUI", 2);
        }
        else
        {
            Debug.LogWarning($"No hay un sprite asignado para el dígito {unitDigit}");
            //unitDigitImageRound.gameObject.SetActive(false);
        }
    }
    public void UIBetweenWavesTimer(float inBetweenRoundsTimer)
    {
        Dialogue roundDialogue = UiRoundCounter.GetComponent<Dialogue>();
        if (roundDialogue.FinishDialogue)
        {
            //UiBetweenWavesTimer.text = $"{Mathf.FloorToInt(inBetweenRoundsTimer / 60)}:{Mathf.FloorToInt(inBetweenRoundsTimer % 60)}";
            UiBetweenWavesTimer.text = $"{Mathf.FloorToInt(inBetweenRoundsTimer)}";

        }
    }
    public void UIBetweenWaves(float waveTimer)
    {
        //UiWaveTimer.text = $"{Mathf.FloorToInt(waveTimer / 60)} : {Mathf.FloorToInt(waveTimer % 60)}";
        UiWaveTimer.text = $"{Mathf.FloorToInt(waveTimer)}";

    }

    public void UIActualWave(int currentWave)
    {
        UiWaveCounter.text = $"Oleada: {currentWave} /3";
    }

    public void UIEnemiesAlive(int enemiesAlive)
    {
        UiEnemyCounter.text = $"Enemigos Restantes: {enemiesAlive}";
    }

    public void UIInstructionToPass(string instruction)
    {
        //sisas
        UiInstructionToPassRound.text = $"{instruction}";
    }

    public void SetPlayerName()
    {
        foreach (var playerNameGO in playerNameText)
        {
            TextMeshProUGUI playerName = playerNameGO.GetComponent<TextMeshProUGUI>();
            playerName.text = inputFieldName.text;


        }
        playerNameProfile = inputFieldName.text;
    }
    public string GetPlayerName()
    {
        AuthenticationService.Instance.UpdatePlayerNameAsync(playerNameProfile);
        return playerNameProfile;
    }
    public void SetCameraCanva(int indexPanel)
    {
        uiPanels[indexPanel].GetComponent<Canvas>().worldCamera = Camera.main;
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
