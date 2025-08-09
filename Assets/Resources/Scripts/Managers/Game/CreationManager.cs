using System;
using System.Collections;
using System.Collections.Generic;
using GameResources.Project.Scripts.Utilities.Audio;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;

namespace Tcp4
{
    public class CreationManager : Singleton<CreationManager>
    {
        public event Action OnChangeInventory;
        [HideInInspector] public List<BaseProduct> Ingredients = new(2);
        public int lastIdInteracted = -1;

        public List<CreationArea> creationAreas = new();

        private Drink newDrink;

        public bool CanAdd() { return Ingredients.Count < 3; }

        public void SelectProduct(BaseProduct pd)
        {
            if (CanAdd())
            {
                //Fazendo o request de sfx
                SoundEventArgs sfxArgs = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "servindo", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                    VolumeScale = .7f // Escala de volume (opcional, padrão é 1f)
                };

                SoundEvent.RequestSound(sfxArgs);
                StorageManager.Instance.playerInventory.RemoveProduct(pd, 1);
                AddIngredient(pd);

                // CORREÇÃO: Notifica mudanças na UI
                OnChangeInventory?.Invoke();
            }
        }

        public void NotifyInventoryUpdate() { OnChangeInventory?.Invoke();}

        public void UnselectProduct(BaseProduct pd)
        {
            StorageManager.Instance.playerInventory.AddProduct(pd, 1);
            RemoveIngredient(pd);

            // CORREÇÃO: Notifica mudanças na UI
            OnChangeInventory?.Invoke();

        }

        void AddIngredient(BaseProduct pd)
        {
            if (CanAdd())
            {
                Ingredients.Add(pd);
                return;
            }
        }

        void RemoveIngredient(BaseProduct pd)
        {
            Ingredients.Remove(pd);
            return;
        }

        private void Start()
        {
            SetupCreationAreas();
        }

        public void SetupCreationAreas()
        {
            for (int i = 0; i < creationAreas.Count; i++)
            {
                creationAreas[i].index = i;
            }
        }

        public void SetupCreationArea(CreationArea areaToAdd)
        {
            creationAreas.Add(areaToAdd);

            for (int i = 0; i < creationAreas.Count; i++)
            {
                creationAreas[i].index = i;
            }
        }

        public void FinishDrink()
        {
            ShopManager.Instance.SpawnCup(newDrink);

            Ingredients.Clear();

            // CORREÇÃO: Atualiza UI após limpar ingredientes
            OnChangeInventory?.Invoke();
            UIManager.Instance.UpdateIngredientsView();

            newDrink = null;
        }

        public void Create()
        {

            if (Ingredients.Count < 3)
            {
                SoundEventArgs sfxArgs1 = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "erro", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                    VolumeScale = .8f, // Escala de volume (opcional, padrão é 1f)

                };
                SoundEvent.RequestSound(sfxArgs1);
                return;
            }

            newDrink = RefinementManager.Instance.CreateDrink(Ingredients);

            Debug.Log($"{newDrink} criado!");

            if (newDrink != null)
            {
                creationAreas[lastIdInteracted].StartPrepare(newDrink.preparationTime);
                UIManager.Instance.ControlCreationMenu(false);
            }
            else
            {
                // Som de erro
                SoundEventArgs sfxArgs = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "erro",
                    VolumeScale = .8f
                };
                SoundEvent.RequestSound(sfxArgs);

                Debug.Log("Drink está nulo e nao pode ser servido!");

                OnChangeInventory?.Invoke();
            }

        }
    }
}