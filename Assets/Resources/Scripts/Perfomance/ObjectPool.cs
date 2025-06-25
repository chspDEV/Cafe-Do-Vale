using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private readonly Queue<GameObject> pool = new Queue<GameObject>();
    private readonly Transform parent;

    public ObjectPool(Transform parent = null)
    {
        this.parent = parent;
    }

    public void AddPool(GameObject[] prefab)
    {
        for (int i = 0; i < prefab.Length; i++)
        {
            GameObject obj = Object.Instantiate(prefab[i]);
            //obj.transform.parent = parent;
            obj.transform.position = parent.transform.position;
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get(GameObject prefab)
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = GameObject.Instantiate(prefab, parent);
            return obj;
        }
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
