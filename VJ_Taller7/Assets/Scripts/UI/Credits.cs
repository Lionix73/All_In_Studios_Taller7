using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Credits : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 80f;
    [SerializeField] private int creditsDuration = 40;

    private RectTransform rectTransform;
    private Vector2 originalPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    void OnEnable()
    {
        rectTransform.anchoredPosition = originalPosition;
        //MoveCredits();
    }

    private void Update()
    {
        rectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
    }

    // esto no funciono por algun motivo
    private IEnumerator MoveCredits()
    {
        float timer = 0f;
        while (timer < creditsDuration)
        {
            rectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }
}