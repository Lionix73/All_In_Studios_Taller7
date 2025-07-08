using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class SeeEnemiesToggle : MonoBehaviour
{
    #region -----Visible Variables-----
    [Header("Renderer")]
    [Tooltip("URP Data on the settings. (PC_Renderer)")]
    [SerializeField] private UniversalRendererData rendererData;

    [Header("See Enemies Effect")]
    [Tooltip("How long before the user can use the effect again.")]
    [SerializeField][Range(0, 10)] private float cooldownDuration = 1.0f;
    [Tooltip("How long before the effect is active.")]
    [SerializeField][Range(0, 10)] private float effectDuration = 5.0f;

    [Header("Scan Effect")]
    [Tooltip("How long the scan effect lasts.")]
    [SerializeField][Range(0, 10)] private float scanDuration = 3.0f;
    [Tooltip("The radius of the scan effect.")]
    [SerializeField][Range(10, 500)] private int size = 100;
    #endregion

    #region -----Private Variables-----
    private RenderObjects _seeEnemiesFeature;
    private ParticleSystem _scanPS;
    private bool _isCooldown;
    #endregion

    private void Awake()
    {
        _scanPS = GetComponentInChildren<ParticleSystem>();

        var main = _scanPS.main;
        main.startLifetime = scanDuration;
        main.startSize = size;
    }

    public void OnTogglePerformed(InputAction.CallbackContext context)
    {
        if (_isCooldown) return;

        if (context.performed)
            StartCoroutine(ToggleWithCooldown());
    }

    private IEnumerator ToggleWithCooldown()
    {
        _isCooldown = true;

        if (_seeEnemiesFeature == null)
        {
            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature is RenderObjects myFeature)
                {
                    if (feature.name == "SeeEnemiesBehindWalls")
                    {
                        _seeEnemiesFeature = myFeature;
                        break;
                    }
                }
            }
        }

        _scanPS.Play();
        _seeEnemiesFeature.SetActive(true);
        yield return new WaitForSeconds(effectDuration);
        _seeEnemiesFeature.SetActive(false);

        yield return new WaitForSeconds(cooldownDuration);
        _isCooldown = false;
    }
}
