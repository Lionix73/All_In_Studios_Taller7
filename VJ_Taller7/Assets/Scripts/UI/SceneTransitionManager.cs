using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingCanvas; // Asigna tu canvas de carga en el inspector
    [SerializeField] private Image loadingProgressBar; // Opcional: barra de progreso
    [SerializeField] private TextMeshProUGUI loadingText; // Opcional: texto de carga

    public static SceneTransitionManager instance;
    private AsyncOperation loadingOperation;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Asegurarse que el canvas de carga está desactivado al inicio
            if (loadingCanvas != null)
                loadingCanvas.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Método para cambiar de escena con pantalla de carga
    public void LoadSceneWithLoadingScreen(string sceneName)
    {
        // Activar el canvas de carga
        if (loadingCanvas != null)
            loadingCanvas.SetActive(true);

        // Opcional: resetear la barra de progreso
        if (loadingProgressBar != null)
            loadingProgressBar.fillAmount = 0f;

        // Opcional: actualizar texto
        if (loadingText != null)
            loadingText.text = "Cargando... 0%";

        // Forzar un renderizado inmediato del canvas de carga
        Canvas.ForceUpdateCanvases();

        // Comenzar la carga asíncrona
        loadingOperation = SceneManager.LoadSceneAsync(sceneName);
        loadingOperation.allowSceneActivation = false; // Controlamos manualmente la activación

        // Configurar para que no se destruya este objeto durante la carga
        //DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (loadingOperation != null && loadingCanvas != null && loadingCanvas.activeSelf)
        {
            // Calcular progreso (LoadAsync llega solo al 0.9 cuando allowSceneActivation es false)
            float progress = Mathf.Clamp01(loadingOperation.progress / 0.9f);

            // Actualizar UI
            if (loadingProgressBar != null)
                loadingProgressBar.fillAmount = progress;

            if (loadingText != null)
                loadingText.text = $"Cargando... {Mathf.Round(progress * 100)}%";

            // Cuando la carga esté casi completa (>= 0.9), activar la escena
            if (progress >= 0.9f)
            {
                loadingOperation.allowSceneActivation = true;
                loadingOperation = null;

                // Desactivar el canvas de carga después de un pequeño delay
                // para asegurar que la nueva escena está completamente activa
                Invoke("HideLoadingScreen", 0.1f);
            }
        }
    }

    private void HideLoadingScreen()
    {
        if (loadingCanvas != null)
            loadingCanvas.SetActive(false);
    }

    // Método estático para fácil acceso desde otros scripts
    public static void LoadScene(string sceneName)
    {
        if (instance != null)
        {
            instance.LoadSceneWithLoadingScreen(sceneName);
        }
        else
        {
            // Si no hay instancia, cargar normalmente
            SceneManager.LoadScene(sceneName);
        }
    }
}
