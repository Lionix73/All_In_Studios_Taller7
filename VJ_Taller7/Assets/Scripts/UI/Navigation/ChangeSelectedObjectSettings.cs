using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSelectedObjectSettings : MonoBehaviour
{   
    private UIManager _ui;
    private Button _button;
    private DeactivateButtons _deactivateButtons;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _deactivateButtons = FindFirstObjectByType<DeactivateButtons>();
    }

    private void Start()
    {
        _button.onClick.AddListener(DelayToChangeTarget);
        _ui = UIManager.Singleton;
    }

    private void DelayToChangeTarget()
    {
        StartCoroutine(ChangeObject());
    }

    private IEnumerator ChangeObject()
    {
        if (_ui.IsMainMenu)
        {
            _deactivateButtons.ChangeButtons(0);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            _deactivateButtons.ChangeButtons(4);
        }

        yield return null;
    }
}
