using System;
using UnityEngine;

[Serializable]
public struct FloatDampenerPhoton
{
    [SerializeField] private float smoothTime;
    private float currentVelocity;

    public float CurrentValue { get; private set; }
    public float TargetValue { get; set; } //metodo de encapsulamiento

    public void Update()
    {
        CurrentValue = Mathf.SmoothDamp(CurrentValue, TargetValue, ref currentVelocity, smoothTime);
    }
}
