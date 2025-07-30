using UnityEngine;
using FMODUnity;

public class Target : MonoBehaviour
{
    [SerializeField] private EventReference hitTargetSound;
    [SerializeField] private Color emissiveRed = Color.red;
    [SerializeField] private Color emissiveGreen = Color.green;
    [SerializeField] private Renderer[] emissiveLights;

    private AimZoneManager targetsManager;
    private bool activated;

    private void Awake()
    {
        targetsManager = FindFirstObjectByType<AimZoneManager>();
        SetEmissiveColor(emissiveRed);
    }

    private void SetEmissiveColor(Color color)
    {
        foreach (var rend in emissiveLights)
        {
            if (rend.material.HasProperty("_EmissionColor"))
            {
                rend.material.SetColor("_EmissionColor", color);
                rend.material.EnableKeyword("_EMISSION");
            }
        }
    }

    public void TargetHit()
    {
        RuntimeManager.PlayOneShot(hitTargetSound, transform.position);

        if (!activated)
        {
            activated = true;
            SetEmissiveColor(emissiveGreen);
            targetsManager.TargetHit();
        }
    }
}