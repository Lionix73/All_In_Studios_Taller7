using Unity.Netcode;
using UnityEngine;

public class PoolableObjectMulti : NetworkBehaviour
{
    public ObjectPoolMulti Parent;
    private bool localActiveState = true;

    private void Start()
    {
        if (IsClient && localActiveState != gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
    public virtual void OnDisable()
    {
        if (!IsServer) return;
        Debug.Log("EnemigoDesactivado");
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