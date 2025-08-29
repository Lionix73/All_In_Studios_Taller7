using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsNavigation : MonoBehaviour
{
    [SerializeField] private InputActionReference next;
    [SerializeField] private InputActionReference previous;
    [SerializeField] private List<SettingSection> sections;

    private int _index = 0;

    private void OnEnable()
    {
        if (next != null && next.action != null)
        {
            next.action.started += _ => NextSection();
        }
        if (previous != null && previous.action != null)
        {
            previous.action.started += _ => PreviousSection();
        }
    }

    private void OnDisable()
    {
        if (next != null && next.action != null)
        {
            next.action.started -= _ => NextSection();
        }
        if (previous != null && previous.action != null)
        {
            previous.action.started -= _ => PreviousSection();
        }
    }

    #region Navigation
    public void NextSection()
    {
        int nextIndex = _index + 1;
        if (nextIndex > sections.Count - 1)
        {
            return;
        }
        else if (nextIndex > 1 && nextIndex < sections.Count - 1)
        {
            sections[_index - 1].sectionButton.gameObject.SetActive(false);
            sections[_index + 2].sectionButton.gameObject.SetActive(true);

            _index++;
            CallClickEvent();
        }
        else
        {
            _index++;
            CallClickEvent();
        }
    }

    public void PreviousSection()
    {
        int nextIndex = _index - 1;
        if (nextIndex < 0)
        {
            return;
        }
        else if (nextIndex < 2 && nextIndex > 0)
        {
            sections[_index + 1].sectionButton.gameObject.SetActive(false);
            sections[_index - 2].sectionButton.gameObject.SetActive(true);

            _index--;
            CallClickEvent();
        }
        else
        {
            _index--;
            CallClickEvent();
        }
    }
    #endregion

    #region Manage Sections
    private void CallClickEvent()
    {
        sections[_index].sectionButton.GetComponent<Button>().onClick.Invoke();
    }

    public void ChangeSection(int index)
    {
        _index = index;
        SetSections();
    }

    private void SetSections()
    {
        for (int i = 0; i < sections.Count; i++)
        {
            if (i == _index)
            {
                sections[i].sectionContent.SetActive(true);
            }
            else
            {
                sections[i].sectionContent.SetActive(false);
            }
        }
    }
    #endregion
}

[System.Serializable]
public struct SettingSection
{
    public GameObject sectionButton;
    public GameObject sectionContent;
}
