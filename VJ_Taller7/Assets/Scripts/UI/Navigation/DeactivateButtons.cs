using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeactivateButtons : MonoBehaviour
{
    [SerializeField] private int activeGroup = 0;
    public int ActiveGroupUI { get => activeGroup; set => activeGroup = value; }

    [Space]
    [SerializeField] private List<InteractableUI> interactables;

    public void ChangeButtons(int indexNewActiveGroup)
    {
        StartCoroutine(SetButtons(indexNewActiveGroup));
    }

    private IEnumerator SetButtons(int index)
    {
        foreach (var item in interactables[index].UiObjs)
        {
            item.SetActive(true);
        }

        yield return new WaitForSeconds(0.15f);

        foreach (var item in interactables[activeGroup].UiObjs)
        {
            item.SetActive(false);
        }

        activeGroup = index;
        EventSystem.current.currentSelectedGameObject = interactables[index].UiObjs[0];
    }
}

[System.Serializable]
public class InteractableUI
{
    public GameObject[] UiObjs;
}
