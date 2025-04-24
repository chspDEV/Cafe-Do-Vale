using System;
using System.Collections.Generic;
using ComponentUtils.ComponentUtils.Scripts;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;


namespace Tcp4
{
    public class CreationManager : Singleton<CreationManager>
    {
        public event Action OnChangeInventory;
        public List<BaseProduct> Ingredients = new(2);
        

        public bool CanAdd() {return Ingredients.Count < 3;}
        public void SelectProduct(BaseProduct pd)
        {
            if(CanAdd())
            {
                SoundManager.PlaySound(SoundType.servindo, 0.2f);
                StorageManager.Instance.playerInventory.RemoveProduct(pd, 1);
                AddIngredient(pd);
                OnChangeInventory?.Invoke();
            }
        }

        public void UnselectProduct(BaseProduct pd)
        {
            StorageManager.Instance.playerInventory.AddProduct(pd, 1);
            RemoveIngredient(pd);
            OnChangeInventory?.Invoke();
        }

        void AddIngredient(BaseProduct pd)
        {
            if(CanAdd())
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

        public void Create()
        {
            Drink newDrink = RefinamentManager.Instance.CreateDrink(Ingredients);

            Debug.Log($"{newDrink} criado!");

            if(newDrink != null)
            {
                //ClientManager.Instance.ServeClient(newDrink);

                ShopManager.Instance.SpawnCup(newDrink);

                Ingredients.Clear();

                OnChangeInventory?.Invoke();
            }
            else
            {
                Debug.Log("Drink está nulo e nao pode ser servido!");
            }
            
        }
    }


}