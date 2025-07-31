using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class AimZoneManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI targetsText;
    public UnityEvent HitAllTargets;

    private Target[] targets;
    private int targetsHit = 0;

    void Awake()
    {
        targets = FindObjectsByType<Target>(FindObjectsSortMode.None);
    }

    public void TargetHit()
    {
        targetsHit++;
        targetsText.text = $"Targets  {targetsHit} / {targets.Length}";

        if (targetsHit >= targets.Length)
        {
            HitAllTargets?.Invoke();
        }
    }
}
