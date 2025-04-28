using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    public ObjectPool Parent;

    public virtual void OnDisable()
    {
        //Debug.Log("OnDisable: " + gameObject.name);

        if (Parent != null)
        {
            //Debug.Log("Returning to pool: " + gameObject.name);
            Parent.ReturnObjectToPool(this);
        }
    }
}