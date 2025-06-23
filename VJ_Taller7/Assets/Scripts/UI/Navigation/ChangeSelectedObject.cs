using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChangeSelectedObject : MonoBehaviour
{
    [SerializeField] private GameObject objectToSelect;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(ChangeObject);
    }

    public void ChangeObject()
    {
        EventSystem.current.currentSelectedGameObject = objectToSelect;
    }
}
