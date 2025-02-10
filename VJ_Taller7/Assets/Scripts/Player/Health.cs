using TMPro;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthDisplay;
    [SerializeField] private float currentHealth = 20f;
    [SerializeField] private float maxHealth = 100f;

    void Update()
    {
        healthDisplay.SetText(currentHealth + "/" + maxHealth);
    }

    public float GetHealth
    {
        get { return currentHealth; }

        set { currentHealth += value; }
    }
}
