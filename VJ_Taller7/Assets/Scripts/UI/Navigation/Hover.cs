using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent HoverEnter;
    public UnityEvent HoverExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
        HoverEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(null);
        HoverExit?.Invoke();
    }
}