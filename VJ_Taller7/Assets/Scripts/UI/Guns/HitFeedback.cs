using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HitFeedback : MonoBehaviour
{
    [Header("Hit Feedback")]
    [SerializeField] private float expandScale = 1.5f;
    [SerializeField] private float expandDuration = 0.1f;
    [SerializeField] private float shrinkDuration = 0.1f;

    private Vector3 originalScale;
    private Coroutine coroutineHit;
    private Image hitMarker;

    private void Awake()
    {
        hitMarker = GetComponent<Image>();
    }

    void Start()
    {
        if (hitMarker != null)
        {
            originalScale = hitMarker.rectTransform.localScale;
            hitMarker.enabled = false;
        }
    }

    public void ShowHitMarker()
    {
        if (coroutineHit != null)
            StopCoroutine(coroutineHit);

        coroutineHit = StartCoroutine(AnimateHitMarker());
    }

    private IEnumerator AnimateHitMarker()
    {
        hitMarker.enabled = true;

        // Expand
        float t = 0f;
        while (t < expandDuration)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(1f, expandScale, t / expandDuration);
            hitMarker.rectTransform.localScale = originalScale * scale;
            yield return null;
        }

        // Shrink
        t = 0f;
        while (t < shrinkDuration)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(expandScale, 1f, t / shrinkDuration);
            hitMarker.rectTransform.localScale = originalScale * scale;
            yield return null;
        }

        hitMarker.rectTransform.localScale = originalScale;
        hitMarker.enabled = false;
        coroutineHit = null;
    }
}
