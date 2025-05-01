using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    public ObjectPool Parent;

    public virtual void OnDisable()
    {
        //Debug.Log("OnDisable: " + gameObject.name);

        if (Parent != null)
        {
            //Debug.Log($"Returning {gameObject.name} to its parent pool.");
            Parent.ReturnObjectToPool(this);
        }
        else
        {
            //Debug.LogWarning($"Parent pool is null for: {gameObject.name}");
        }
    }
}