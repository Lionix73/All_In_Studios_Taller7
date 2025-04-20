using Unity.Netcode;
using UnityEngine;

public class PoolableObjectMulti : NetworkBehaviour
{
    public ObjectPoolMulti Parent;

    public virtual void OnDisable()
    {
        if (!IsServer) return;

        if (Parent != null)
        {
            Parent.ReturnObjectToPool(this);
        }
    }

    public void Activate()
    {
        if (IsServer)
        {
            ActivateClientRpc();
        }
    }

    public void Deactivate()
    {
        if (IsServer)
        {
            DeactivateClientRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void ActivateClientRpc()
    {
        gameObject.SetActive(true);
    }

    [Rpc(SendTo.Everyone)]
    private void DeactivateClientRpc()
    {
        gameObject.SetActive(false);
    }
}