using UnityEngine;
using UnityEngine.Events;

public class AimZoneManager : MonoBehaviour
{
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

        if (targetsHit >= targets.Length)
        {
            HitAllTargets?.Invoke();
        }
    }
}
