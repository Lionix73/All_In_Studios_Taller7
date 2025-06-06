using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnomatopoeiaController : MonoBehaviour
{
    [Header("Onomatopeyas")]
    [Tooltip("Lista de sprites de onomatopeyas")]
    public List<Sprite> onomatopoeiaSprites;

    [Header("Animación")]
    [SerializeField] private float expandScale = 1.5f;
    [SerializeField] private float expandDuration = 0.1f;
    [SerializeField] private float cooldown = 0.5f;

    [Header("Probabilidad")]
    [Range(0f, 1f)] public float probability = 0.3f;

    private Vector3 originalScale;
    private Coroutine animationCoroutine;
    private Image onomatopoeiaImage;

    private void Awake()
    {
        onomatopoeiaImage = GetComponent<Image>();
    }

    private void Start()
    {
        if (onomatopoeiaImage != null)
        {
            originalScale = onomatopoeiaImage.rectTransform.localScale;
            onomatopoeiaImage.enabled = false;
        }
    }

    /// <summary>
    /// Intenta mostrar una onomatopeya según la probabilidad definida.
    /// </summary>
    public void TryShowOnomatopoeia()
    {
        if (Random.value <= probability)
        {
            ShowRandomOnomatopoeia();
        }
    }

    private void ShowRandomOnomatopoeia()
    {
        if (onomatopoeiaSprites.Count == 0 || onomatopoeiaImage == null)
            return;

        // Seleccionar un sprite aleatorio
        int index = Random.Range(0, onomatopoeiaSprites.Count);
        onomatopoeiaImage.sprite = onomatopoeiaSprites[index];

        // Cancelar animación anterior si existe
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        // Iniciar nueva animación
        animationCoroutine = StartCoroutine(AnimateOnomatopoeia());
    }

    private IEnumerator AnimateOnomatopoeia()
    {
        onomatopoeiaImage.enabled = true;
        RectTransform rect = onomatopoeiaImage.rectTransform;

        // Expandir
        float t = 0f;
        while (t < expandDuration)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(1f, expandScale, t / expandDuration);
            rect.localScale = originalScale * scale;
            yield return null;
        }

        yield return new WaitForSeconds(cooldown);

        onomatopoeiaImage.enabled = false;
        rect.localScale = originalScale;
        animationCoroutine = null;
    }
}
