using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiPlayerState : NetworkBehaviour
{
    private NetworkVariable <bool> isReady = new NetworkVariable<bool> ();
    public bool IsReady
    {
        get { return isReady.Value; }
        set { if(IsServer) isReady.Value = value; }
    }

    public delegate void PlayerIsReady();
    public event PlayerIsReady OnPlayerReady; //Este evento es para avisar al player manager, luego ese avisa a todos


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        IsReady = false;

    }

    public void OnReady(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (IsReady) return;

        if(context.started)
        {
            ChangePlayerStateRpc();
        }
    }
    [Rpc(SendTo.Server)]
    public void ChangePlayerStateRpc()
    {
        IsReady = true;
        OnPlayerReady?.Invoke();

    }

}
public enum PlayerState
{
    Wait,
    Playing,
    End
}
