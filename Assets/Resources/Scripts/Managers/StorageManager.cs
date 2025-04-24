using System;
using System.Collections.Generic;
using ComponentUtils.ComponentUtils.Scripts;
using Tcp4.Assets.Resources.Scripts.Systems.Clients;
using Tcp4.Assets.Resources.Scripts.Systems.Collect_Cook;
using UnityEngine;
using UnityEngine.UI;

namespace Tcp4
{
    public class StorageManager : Singleton<StorageManager>
    {
        [SerializeField] private StorageArea currentStorage;
        public Inventory playerInventory;
        public GameObject pfSlot;

        public event Action OnChangeStorage;
        public event Action OnCleanStorage;

        public void Start()
        {
            playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
        }
        public void SetupCurrentStorage(StorageArea newStorage)
        {
            currentStorage = newStorage;
            OnChangeStorage.Invoke();
        }

        public void ClearSlots()
        {
            OnCleanStorage.Invoke();
        }
        public StorageArea GetStorageArea()
        {
            if(currentStorage != null)
            {
                return currentStorage;
            }

            Debug.LogError("Storage nulo!");
            return null;
        }

        public void TransferItems()
        {
            var storageInventory = currentStorage.inventory;

            if(playerInventory == null || storageInventory == null) return;

            bool isAbleToTransfer = playerInventory.CountItem(currentStorage.item) > 0;

            if (isAbleToTransfer)
            {
                SoundManager.PlaySound(SoundType.colocando, 0.05f);
                playerInventory.RemoveProduct(currentStorage.item, 1);
                storageInventory.AddProduct(currentStorage.item, 1);
            }

            OnChangeStorage.Invoke();
        }

        public void GetItems()
        {
            var storageInventory = currentStorage.inventory;

            if(playerInventory == null || storageInventory == null || !playerInventory.CanStorage()) return;

            bool isAbleToTransfer = storageInventory.CountItem(currentStorage.item) > 0;

            if (isAbleToTransfer)
            {
                playerInventory.AddProduct(currentStorage.item, 1);
                storageInventory.RemoveProduct(currentStorage.item, 1);
            }

            OnChangeStorage.Invoke();
        }

    }
}
