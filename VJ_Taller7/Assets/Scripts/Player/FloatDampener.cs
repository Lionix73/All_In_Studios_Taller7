using System;
using UnityEngine;

[Serializable]
public struct FloatDampener
{
    [SerializeField] private float smoothTime;
    private float currentVelocity;

    public float CurrentValue { get; private set; }
    public float TargetValue { get; set; }
    public float SmoothTime { get; set; }

    public void Update()
    {
        CurrentValue = Mathf.SmoothDamp(CurrentValue, TargetValue, ref currentVelocity, smoothTime);
    }
}
