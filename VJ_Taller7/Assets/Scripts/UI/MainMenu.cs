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
                Destroy(value);
                Debug.LogError($"There should only ever be one instance of {nameof(UIManager)}");
            }
        }
    }

    public static UIManager _singleton;

    public void Awake()
    {
        Singleton = this;
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
        if (screens[0].activeSelf)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

    }

    [SerializeField] private bool IsPaused = false;
    [SerializeField] GameObject[] screens;
    [SerializeField] int activeScene;
    [SerializeField] Image healthBar;

    [SerializeField] Image gunImage;
    [SerializeField] TextMeshProUGUI gunTypeText;
    [SerializeField] TextMeshProUGUI ammoText;
    [SerializeField] TextMeshProUGUI maxTotalAmmoText;

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
        IsPaused = !IsPaused;
        screens[indexPauseScreen].SetActive(IsPaused);

    }

    public void PlayScene(int indexInGameUI)
    {
        screens[0].SetActive(false);
        screens[indexInGameUI].SetActive(true);
    }
    public void BackToMenu(int indexInGameUI)
    {
        screens[0].SetActive(true);
        screens[indexInGameUI].SetActive(false);
    }

    public void GetPlayerHealth(float playerHealth, float maxHealth)
    {
        healthBar.fillAmount = Mathf.Clamp(playerHealth / maxHealth, 0, 1);
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

}
