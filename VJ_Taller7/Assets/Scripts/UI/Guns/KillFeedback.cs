using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KillFeedback : MonoBehaviour
{
    [Header("Kill Feedback")]
    [SerializeField] private float expandScale = 2.0f;
    [SerializeField] private float expandDuration = 0.3f;

    private Vector3 originalScale;
    private Coroutine coroutineHit;
    private RoundManager roundManager;
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

        roundManager = FindFirstObjectByType<RoundManager>();
        roundManager.OnEnemyKilled += ShowKillMarker;
    }

    public void ShowKillMarker()
    {
        if (coroutineHit != null)
            StopCoroutine(coroutineHit);

        coroutineHit = StartCoroutine(AnimateKillMarker());
    }

    private IEnumerator AnimateKillMarker()
    {
        hitMarker.enabled = true;

        hitMarker.rectTransform.DOScale(expandScale, expandDuration); // Expand Scale
        yield return new WaitForSeconds(expandDuration);

        hitMarker.DOFade(0f, 0.3f); // Fade Image
        yield return new WaitForSeconds(0.3f);

        hitMarker.enabled = false;

        hitMarker.rectTransform.localScale = originalScale;
        hitMarker.DOFade(1f, 0f); // Return to alpha = 1
        coroutineHit = null;
    }
}
