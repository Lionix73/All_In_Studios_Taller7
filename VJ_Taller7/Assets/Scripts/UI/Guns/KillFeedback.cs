using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KillFeedback : MonoBehaviour
{
    [Header("Kill Feedback")]
    [SerializeField] private float expandScale = 2.0f;
    [SerializeField] private float expandDuration = 0.3f;
    [SerializeField] private float shrinkDuration = 0.2f;

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
