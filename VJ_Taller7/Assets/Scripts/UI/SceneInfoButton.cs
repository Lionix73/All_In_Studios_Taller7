using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class SceneInfoButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameSceneUI;
    [SerializeField] Image imageScene;
    [SerializeField] string nameScene;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSceneData(SceneScriptableObject sceneData)
    {
        nameScene = sceneData.nameScene;

        if (nameSceneUI != null)
        {
            nameSceneUI.text = sceneData.displayName;
        }

        if (imageScene != null)
        {
            imageScene.sprite = sceneData.imageScene;
        }
    }

    public void SelectScene()
    {
        SceneManager.LoadScene(nameScene);
    }
}
