using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    public ObjectPool Parent;

    public virtual void OnDisable()
    {
        if (Parent != null)
        {
            Parent.ReturnObjectToPool(this);
        }
    }
}