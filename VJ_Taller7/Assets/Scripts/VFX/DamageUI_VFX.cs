using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public class DamageUI_VFX : MonoBehaviour
{
    [SerializeField] private Image damageImage;
    [SerializeField] private float duration = 1.0f;
    public float Duration { get => duration; set => duration = value; }

    [SerializeField] private float flashDuration = 0.2f;
    public float FlashDuration { get => flashDuration; set => flashDuration = value; }

    [SerializeField] private float flashIntensity = 0.5f;
    public float FlashIntensity { get => flashIntensity; set => flashIntensity = value; }

    [SerializeField] private int healthThreshold = 30;
    public int HealthThreshold { get => healthThreshold; set => healthThreshold = value; }

    [SerializeField] private Color damageColor = new Color(1, 0, 0, 0);

    void Start()
    {
        if(damageImage == null)
        {
            damageImage = GetComponent<Image>();
        }

        damageImage.color = damageColor;
    }

    public void UpdateDamageEffect(float health, float maxHealth)
    {
        if (health < healthThreshold)
        {
            float alpha = Mathf.Clamp01((healthThreshold - health) / healthThreshold);
            damageImage.DOFade(alpha, duration);
        }
        else
        {
            damageImage.DOFade(0, duration);
        }
    }

    public void ShowDamageFlash(float health, float maxHealth)
    {
        damageImage.DOFade(0.5f, flashDuration).OnComplete(() => 
        {
            damageImage.DOFade(0, flashDuration);
        });
    }
}
