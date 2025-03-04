using UnityEngine;

[CreateAssetMenu(fileName = "Scaling Configuration", menuName = "Enemies/Scaling Configuration")]
public class ScalingScriptableObject : ScriptableObject
{
    public AnimationCurve healthCurve;
    public AnimationCurve damageCurve;
    public AnimationCurve speedCurve;
    public AnimationCurve spawnRateCurve;
    public AnimationCurve spawnCountCurve;
}
