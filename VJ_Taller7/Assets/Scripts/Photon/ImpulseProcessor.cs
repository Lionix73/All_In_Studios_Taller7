using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion.Addons.KCC;
public class ImpulseProcessor : KCCProcessor
{
    public float ImpulseStrength;
    [SerializeField] private bool isSphere;
    public override void OnEnter(KCC kcc, KCCData data)
    {
        Vector3 impulseDirection;

        impulseDirection = transform.forward;
        //Clear dynamic velocity proportionally to impulse direction
        kcc.SetDynamicVelocity(data.DynamicVelocity - Vector3.Scale(data.DynamicVelocity, impulseDirection.normalized));
        //Add impulse 
        kcc.AddExternalImpulse(impulseDirection * ImpulseStrength);
    }
}