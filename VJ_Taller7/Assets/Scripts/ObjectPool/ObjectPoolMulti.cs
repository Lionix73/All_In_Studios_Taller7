using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolMulti
{
    private GameObject parent;
    private PoolableObjectMulti Prefab;
    public int Size {get; private set;}
    private List<PoolableObjectMulti> AvailableObjectsPool;
    private static Dictionary<PoolableObjectMulti, ObjectPoolMulti> ObjectPools = new Dictionary<PoolableObjectMulti, ObjectPoolMulti>();

    private ObjectPoolMulti(PoolableObjectMulti Prefab, int Size)
    {
        this.Prefab = Prefab;
        this.Size = Size;
        AvailableObjectsPool = new List<PoolableObjectMulti>(Size);
    }

    public static ObjectPoolMulti CreateInstance(PoolableObjectMulti Prefab, int Size)
    {
        ObjectPoolMulti pool = null;

        if (ObjectPools.ContainsKey(Prefab))
        {
            pool = ObjectPools[Prefab];
        }
        else{
            pool = new ObjectPoolMulti(Prefab, Size);

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
        PoolableObjectMulti poolableObject = GameObject.Instantiate(Prefab, Vector3.zero, Quaternion.identity, parent.transform);
        poolableObject.Parent = this;
        poolableObject.gameObject.SetActive(false);
    }

    public PoolableObjectMulti GetObject()
    {
        if (AvailableObjectsPool.Count <= 0)
        {
            CreateObject();
        } 

        PoolableObjectMulti instance = AvailableObjectsPool[0];

        AvailableObjectsPool.RemoveAt(0);

        instance.gameObject.SetActive(true);

        return instance;
    }

    public void ReturnObjectToPool(PoolableObjectMulti Object)
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