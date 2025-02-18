using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Image progressImage;
    [SerializeField] private float defaultSpeed = 1f;
    [SerializeField] private Gradient colorGradient;
    [SerializeField] private UnityEvent<float> onProgress;
    [SerializeField] private UnityEvent onComplete;

    private Coroutine animationCoroutine;

    void Start()
    {
        if(progressImage.type != Image.Type.Filled){
            Debug.LogError("Progress Image type must be set to Filled");
            enabled = false;

            #if UNITY_EDITOR
                EditorGUIUtility.PingObject(this);
            #endif
        }
    }

    public void SetProgress(float progress){
        SetProgress(progress, defaultSpeed);
    }

    public void SetProgress(float progress, float speed){
        if(progress < 0 || progress > 1){
            Debug.LogWarning($"Progress value must be between 0 and 1, got: {progress}");
            progress = Mathf.Clamp01(progress);
        }

        if(progress != progressImage.fillAmount){
            if(animationCoroutine != null){
                StopCoroutine(animationCoroutine);
            }

            animationCoroutine = StartCoroutine(AnimateProgress(progress, speed));
        }
    }

    private IEnumerator AnimateProgress(float progress, float speed){
        float time = 0;
        float initialProgress = progressImage.fillAmount;

        while (time < 1){
            progressImage.fillAmount = Mathf.Lerp(initialProgress, progress, time);
            time += Time.deltaTime * speed;
            
            progressImage.color = colorGradient.Evaluate(1 - progressImage.fillAmount);

            onProgress?.Invoke(progressImage.fillAmount);
            yield return null;
        }

        progressImage.fillAmount = progress;
        progressImage.color = colorGradient.Evaluate(1 - progressImage.fillAmount);

        onProgress?.Invoke(progress);
        onComplete?.Invoke();
    }   
}
