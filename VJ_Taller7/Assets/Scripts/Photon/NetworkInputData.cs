using Fusion;
using UnityEngine;

public enum InputButton
{
    Jump,
    Slide,
    Crouch,
    Run,
    Dash,
    OnAim,
    Emote
}
public struct NetworkInputData : INetworkInput
{
    public NetworkButtons Buttons;
    public Vector2 Direction;
    public Vector2 LookDelta;
}