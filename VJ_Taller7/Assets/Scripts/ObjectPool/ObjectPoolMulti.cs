using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ObjectPoolMulti
{
    private GameObject parent;
    private PoolableObjectMulti Prefab;
    public int Size {get; private set;}
    private List<PoolableObjectMulti> AvailableObjectsPool;
    private static Dictionary<PoolableObjectMulti, ObjectPoolMulti> ObjectPools = new Dictionary<PoolableObjectMulti, ObjectPoolMulti>();
    private NetworkPrefabsList networkPrefabsList;

    private ObjectPoolMulti(PoolableObjectMulti Prefab, int Size)
    {
        this.Prefab = Prefab;
        this.Size = Size;
        AvailableObjectsPool = new List<PoolableObjectMulti>(Size);
    }

    public static ObjectPoolMulti CreateInstance(PoolableObjectMulti Prefab, int Size)
    {
        ObjectPoolMulti pool = null;
        if (Prefab == null)
        {
            Debug.LogError("Prefab cannot be null");
            return null;
        }

        if (ObjectPools.ContainsKey(Prefab))
        {
            pool = ObjectPools[Prefab];
        }
        else{
            pool = new ObjectPoolMulti(Prefab, Size);

           /* pool.parent = new GameObject(Prefab + " Pool");
            
            // Asegúrate de que el padre tenga NetworkObject
            if (!pool.parent.GetComponent<NetworkObject>())
            {
                var netObj = pool.parent.AddComponent<NetworkObject>();

                netObj.Spawn(); // ¡Importante spawnear el padre!
            }*/

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
        PoolableObjectMulti poolableObject = GameObject.Instantiate(Prefab, Vector3.zero, Quaternion.identity);
        poolableObject.GetComponent<NetworkObject>().Spawn();
        poolableObject.Parent = this;
        poolableObject.Deactivate();
        //poolableObject.transform.SetParent(parent.transform, false);
    }

    public PoolableObjectMulti GetObject()
    {
        if (AvailableObjectsPool.Count == 0)
        {
            CreateObject();
        }

        PoolableObjectMulti instance = AvailableObjectsPool[0];

        AvailableObjectsPool.RemoveAt(0);

        instance.Activate();

        return instance;
    }

    public void ReturnObjectToPool(PoolableObjectMulti Object)
    {
        Debug.Log("Return Object to pool");
        AvailableObjectsPool.Add(Object);
    }

    public static void ClearPools()
    {
        foreach (var pool in ObjectPools.Values)
        {
            // Destruir todos los objetos en AvailableObjectsPool
            foreach (var pooledObject in pool.AvailableObjectsPool)
            {
                if (pooledObject != null && pooledObject.gameObject != null)
                {
                    // Asegurarse de despawnear antes de destruir (importante para Netcode)
                    NetworkObject netObj = pooledObject.GetComponent<NetworkObject>();
                    if (netObj != null && netObj.IsSpawned)
                    {
                        netObj.Despawn();
                    }
                    GameObject.Destroy(pooledObject.gameObject);
                }
            }
            pool.AvailableObjectsPool.Clear();
        }
        ObjectPools.Clear();
    }
    private void RegisterNetworkPrefab(GameObject prefab)
    {
        NetworkObject networkObject = prefab.GetComponent<NetworkObject>();
        if (networkObject == null) return;

        if (!NetworkManager.Singleton != true)
        {
            NetworkManager.Singleton.AddNetworkPrefab(prefab);
        }
    }
}