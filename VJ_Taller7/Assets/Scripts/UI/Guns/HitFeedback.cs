using DG.Tweening;
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

        hitMarker.rectTransform.DOScale(expandScale, expandDuration); // Expand Scale
        yield return new WaitForSeconds(expandDuration);

        hitMarker.rectTransform.DOScale(originalScale, shrinkDuration); // Shrink Scale
        yield return new WaitForSeconds(shrinkDuration);

        hitMarker.DOFade(0f, 0.1f); // Fade Image
        yield return new WaitForSeconds(0.1f);

        hitMarker.enabled = false;
        hitMarker.rectTransform.localScale = originalScale;
        hitMarker.DOFade(1f, 0f); // Return to alpha = 1
        coroutineHit = null;
    }
}
