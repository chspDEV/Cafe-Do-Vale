using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    // Dicionário que mapeia cada prefab para sua própria fila de instâncias
    private readonly Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private readonly Transform parent;

    public ObjectPool(Transform parent = null)
    {
        this.parent = parent;
    }

    public void AddPool(GameObject[] prefabs)
    {
        foreach (GameObject prefab in prefabs)
        {
            // Usa o nome do prefab como chave única
            string key = prefab.name;

            // Se ainda não existe uma fila para este prefab, cria uma
            if (!pools.ContainsKey(key))
            {
                pools[key] = new Queue<GameObject>();
            }

            // Cria uma instância e adiciona à fila específica deste prefab
            GameObject obj = Object.Instantiate(prefab, parent);
            obj.name = prefab.name; // Mantém o nome original (sem "(Clone)")
            obj.transform.position = parent.position;
            obj.SetActive(false);
            pools[key].Enqueue(obj);
        }
    }

    public GameObject Get(GameObject prefab)
    {
        string key = prefab.name;

        // Se existe uma fila para este prefab E ela tem objetos disponíveis
        if (pools.ContainsKey(key) && pools[key].Count > 0)
        {
            GameObject obj = pools[key].Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // Se não há objetos disponíveis, cria um novo
            GameObject obj = Object.Instantiate(prefab, parent);
            obj.name = prefab.name;
            return obj;
        }
    }

    public void Return(GameObject obj)
    {
        string key = obj.name;

        obj.SetActive(false);

        // Adiciona de volta à fila correta
        if (!pools.ContainsKey(key))
        {
            pools[key] = new Queue<GameObject>();
        }

        pools[key].Enqueue(obj);
    }

    // Método útil para debug
    public void DebugPoolStatus()
    {
        foreach (var kvp in pools)
        {
            Debug.Log($"Pool '{kvp.Key}': {kvp.Value.Count} objetos disponíveis");
        }
    }
}