using System;
using System.Collections.Generic;
using GameResources.Project.Scripts.Utilities.Audio;
using Tcp4.Assets.Resources.Scripts.Managers;
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
            OnChangeStorage?.Invoke();
        }

        public void ClearSlots()
        {
            OnCleanStorage?.Invoke();
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
                //Fazendo o request de sfx
                SoundEventArgs sfxArgs = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "interacao", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                    Position = transform.position, // Posição para o som 3D
                    VolumeScale = .5f // Escala de volume (opcional, padrão é 1f)
                };
                SoundEvent.RequestSound(sfxArgs);


                playerInventory.RemoveProduct(currentStorage.item, 1);
                storageInventory.AddProduct(currentStorage.item, 1);
            }
            else
            {
                //Fazendo o request de sfx
                SoundEventArgs sfxArgs = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "erro", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                    Position = transform.position, // Posição para o som 3D
                    VolumeScale = .7f // Escala de volume (opcional, padrão é 1f)
                };
                SoundEvent.RequestSound(sfxArgs);
            }

                OnChangeStorage?.Invoke();
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
            else
            {
                //Fazendo o request de sfx
                SoundEventArgs sfxArgs = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "erro", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                    Position = transform.position, // Posição para o som 3D
                    VolumeScale = .7f // Escala de volume (opcional, padrão é 1f)
                };
                SoundEvent.RequestSound(sfxArgs);
            }

                OnChangeStorage?.Invoke();
        }

    }
}
