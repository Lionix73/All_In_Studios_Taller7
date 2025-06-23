using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChangeSelectedObjectSettings : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuObjectToSelect;
    [SerializeField] private GameObject inGameObjectToSelect;
    
    private UIManager ui;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(ChangeObject);
        ui = UIManager.Singleton;
    }

    public void ChangeObject()
    {
        EventSystem.current.currentSelectedGameObject = ui.IsMainMenu ? mainMenuObjectToSelect : inGameObjectToSelect;
    }
}
