using GameResources.Project.Scripts.Utilities.Audio;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tcp4
{
    public class Inventory : MonoBehaviour
    {
        private int limit = 6;
        public bool canSpawn = true;
        [SerializeField] private List<BaseProduct> productInventory = new();
        [SerializeField] private List<GameObject> instanceInventory = new();
        [SerializeField] private Transform bagPoint;

        public List<BaseProduct> GetInventory() => productInventory;
        public int GetLimit() => limit;

        public bool CanStorage(){return productInventory.Count < limit;}

        public void AddProduct(BaseProduct product, int amount)
        {
            if (amount <= 0 || product == null || !CanStorage())
            {
                Debug.LogError("Erro: quantidade inválida, produto nulo ou inventário cheio.");
                SoundEventArgs sfxArgs = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "erro",
                    Position = transform.position,
                    VolumeScale = 0.3f
                };
                SoundEvent.RequestSound(sfxArgs);
                return;
            }

            for (int i = 0; i < amount; i++)
            {
                productInventory.Add(product);
                QuestManager.Instance.CheckItemCollected(product.productName);

                if(canSpawn)
                    Spawn(product.model);
            }
        }

        public bool AddProductReturn(BaseProduct product, int amount)
        {
            if (amount <= 0 || product == null || !CanStorage())
            {
                Debug.LogError("Erro: quantidade inválida, produto nulo ou inventário cheio.");
                return false;
            }

            for (int i = 0; i < amount; i++)
            {
                productInventory.Add(product);
                QuestManager.Instance.CheckItemCollected(product.productName);

                if (canSpawn)
                    Spawn(product.model);

                return true;
            }

            return false;
        }

        public void RemoveProduct(BaseProduct product, int amount)
        {
            if (amount <= 0 || product == null)
            {
                Debug.LogError("Erro: quantidade inválida ou produto nulo.");
                return;
            }

            for (int i = 0; i < amount; i++)
            {
                BaseProduct itemToRemove = productInventory.Find(x => x.productID == product.productID);
                if (itemToRemove != null)
                {
                    productInventory.Remove(itemToRemove);

                    if (canSpawn)
                        Despawn(product.model);
                }
            }
        }

        public void RefineProduct(BaseProduct product)
        {
            if (product == null)
            {
                Debug.LogError("Erro: produto nulo.");
                return;
            }

            BaseProduct refinedProduct = RefinementManager.Instance.Refine(product);

            if (refinedProduct != null)
            {
                RemoveProduct(product, 1);
                AddProduct(refinedProduct, 1);
            }
        }


        public void UpdateLimit(int newLimit)
        {
            limit = newLimit;
        }
        void Spawn(GameObject model)
        {
            
            GameObject instance = Instantiate(model, bagPoint);
            instanceInventory.Add(instance);
            ReorganizeInventory();
        }

        public int CountItem(BaseProduct itemToCount)
        {
            int counter = 0;

            foreach(var item in productInventory)
            {
                if(item == itemToCount) {counter ++;}
            }

            return counter;

        }
        void Despawn(GameObject model)
        {
            if (instanceInventory.Count == 0) return;

            GameObject instanceToRemove = null;
            int index = -1;

            for (int i = 0; i < instanceInventory.Count; i++)
            {
                if (instanceInventory[i].GetComponent<MeshFilter>().sharedMesh == model.GetComponent<MeshFilter>().sharedMesh)
                {
                    instanceToRemove = instanceInventory[i];
                    index = i;
                    break; // Encontramos a instância, podemos sair do loop
                }
            }

            if (instanceToRemove != null && index != -1)
            {
                Destroy(instanceToRemove);
                instanceInventory.RemoveAt(index);
                ReorganizeInventory();
            }
        }

        void ReorganizeInventory()
        {
            for (int i = 0; i < instanceInventory.Count; i++)
            {
                var offset = i / 5f;
                instanceInventory[i].transform.position = bagPoint.position + new Vector3(0, offset, 0);   
            }
        }
        public void Clear()
        {
            foreach (var instance in instanceInventory)
            {
                Destroy(instance);
            }

            instanceInventory.Clear();
            productInventory.Clear();
        }
    }
}
