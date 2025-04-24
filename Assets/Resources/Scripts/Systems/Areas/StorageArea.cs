using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Tcp4.Assets.Resources.Scripts.Systems.Collect_Cook
{
    public class StorageArea : MonoBehaviour
    {
        public Inventory inventory;
        public BaseProduct item;

        [SerializeField] private float interfaceDelay; 

        private bool isInterfaceOpen;

        private void Start()
        {
            inventory = GetComponent<Inventory>();
            inventory.UpdateLimit(400);
            isInterfaceOpen = false;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (!isInterfaceOpen)
                {
                    StartCoroutine(OpenInterfaceAfterDelay());
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                StopAllCoroutines();
                CloseInterface();
                StorageManager.Instance.ClearSlots();
            }
        }

        private IEnumerator OpenInterfaceAfterDelay()
        {
        
            StorageManager.Instance.SetupCurrentStorage(this);

            yield return new WaitForSeconds(interfaceDelay);

            if (!isInterfaceOpen)
            {
                UIManager.Instance.ControlStorageMenu(true);
                isInterfaceOpen = true;
            }
        }

        private void CloseInterface()
        {
            UIManager.Instance.ControlStorageMenu(false);
            isInterfaceOpen = false;
        }

    }
}
