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
}