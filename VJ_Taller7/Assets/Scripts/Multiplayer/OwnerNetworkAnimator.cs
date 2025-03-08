using Unity.Netcode.Components;
using UnityEngine;

public class OwnerNetworkAnimator : NetworkAnimator
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
