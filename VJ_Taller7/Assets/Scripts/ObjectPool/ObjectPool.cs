using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private GameObject parent;
    private PoolableObject Prefab;
    public int Size {get; private set;}
    private List<PoolableObject> AvailableObjectsPool;
    private static Dictionary<PoolableObject, ObjectPool> ObjectPools = new Dictionary<PoolableObject, ObjectPool>();

    private ObjectPool(PoolableObject Prefab, int Size)
    {
        this.Prefab = Prefab;
        this.Size = Size;
        AvailableObjectsPool = new List<PoolableObject>(Size);
    }

    public static ObjectPool CreateInstance(PoolableObject Prefab, int Size)
    {
        ObjectPool pool = null;

        if (ObjectPools.ContainsKey(Prefab))
        {
            pool = ObjectPools[Prefab];
        }
        else{
            pool = new ObjectPool(Prefab, Size);

            pool.parent = new GameObject(Prefab + " Pool");
            pool.CreateObjects();

            ObjectPools.Add(Prefab, pool);
        }

        return pool;
    }

    private void CreateObjects()
    {
        for (int i = 0; i < Size; i++)
        {
            CreateObject();
        }
    }

    private void CreateObject(){
        PoolableObject poolableObject = GameObject.Instantiate(Prefab, Vector3.zero, Quaternion.identity, parent.transform);
        poolableObject.Parent = this;
        poolableObject.gameObject.SetActive(false);
    }

    public PoolableObject GetObject()
    {
        if (AvailableObjectsPool.Count <= 0)
        {
            CreateObject();
        } 

        PoolableObject instance = AvailableObjectsPool[0];

        AvailableObjectsPool.RemoveAt(0);

        instance.gameObject.SetActive(true);

        return instance;
    }

    public void ReturnObjectToPool(PoolableObject Object)
    {
        AvailableObjectsPool.Add(Object);
    }

    public static void ClearPools()
    {
        foreach (var pool in ObjectPools.Values)
        {
            if (pool.parent != null)
            {
                GameObject.Destroy(pool.parent);
            }
        }
        ObjectPools.Clear();
    }
}