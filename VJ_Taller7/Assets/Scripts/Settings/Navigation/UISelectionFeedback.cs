using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class UISelectionFeedback : MonoBehaviour
{
    [Header("Animación de escala")]
    public float scaleMultiplier = 1.2f;
    public float scaleDuration = 0.2f;

    [Header("Color del Outline")]
    public Color outlineColor = Color.yellow;
    public float outlineWidth = 1.5f;

    private GameObject lastSelected;

    void Update()
    {
        GameObject current = EventSystem.current.currentSelectedGameObject;

        if (current != lastSelected)
        {
            // Restaurar el anterior si existe
            if (lastSelected != null)
            {
                ResetButtonFeedback(lastSelected);
            }

            // Aplicar feedback al nuevo seleccionado
            if (current != null)
            {
                ApplyButtonFeedback(current);
            }

            lastSelected = current;
        }
    }

    private void ApplyButtonFeedback(GameObject obj)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.DOScale(Vector3.one * scaleMultiplier, scaleDuration).SetEase(Ease.OutBack);
        }

        Outline outline = obj.GetComponent<Outline>() ?? obj.AddComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(outlineWidth, outlineWidth);
    }

    private void ResetButtonFeedback(GameObject obj)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.DOScale(Vector3.one, scaleDuration).SetEase(Ease.OutBack);
        }

        Outline outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            Destroy(outline);
        }
    }
}
