using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tcp4.Assets.Resources.Scripts.Systems.Collect_Cook
{
    public class CollectFromStorageArea : MonoBehaviour
    {
        [SerializeField] StorageArea storageArea;

        [SerializeField] private float timeToGive;
        private float currentTime;
        private bool isAbleToGive;


        public void Update()
        {
            if (currentTime > 0 && !isAbleToGive)
            {
                currentTime -= Time.deltaTime;
            }
            else
            {
                currentTime = 0;
                isAbleToGive = true;
            }
        }

        public void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") && isAbleToGive)
            {
                /*
                Inventory i = other.GetComponent<Inventory>();
                List<Ingredients> playerInventory = i.GetInventory();

                if (storageArea.storage.GetInventory().Count <= 0) return;

                Ingredients ingredient = storageArea.storage.GetInventory()[^1];

                

                storageArea.storage.RemoveIngredient(ingredient, 1);
                i.AddIngredient(ingredient, 1);

                isAbleToGive = false;
                currentTime = timeToGive;*/
            }
        }
    }
}