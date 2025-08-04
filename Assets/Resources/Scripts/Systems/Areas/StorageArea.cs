using System.Collections;
using Tcp4.Assets.Resources.Scripts.Managers;
using TMPro;
using UnityEngine;

namespace Tcp4.Assets.Resources.Scripts.Systems.Collect_Cook
{
    public class StorageArea : BaseInteractable
    {
        public Inventory inventory;
        public BaseProduct item;
        public string storageID;

        [SerializeField] private float interfaceDelay;

        private bool isInterfaceOpen;

        [Header("System Integration")]
        public int areaID;


        public void RegisterAreaID()
        {
            // Adicione esta linha para registrar a estação:
            if (WorkerManager.Instance != null && GameAssets.Instance != null)
            {
                areaID = GameAssets.Instance.GenerateAreaID();
                WorkerManager.Instance.RegisterStorageStation(areaID, this);
            }
        }

        public override void Start()
        {
            base.Start();
            inventory = GetComponent<Inventory>();
            inventory.UpdateLimit(400);
            isInterfaceOpen = false;
            interactable_id = "storageArea";

            RegisterAreaID();
        }

        public override void OnLostFocus()
        {
            base.OnLostFocus();
            StopAllCoroutines();
            CloseInterface();
            StorageManager.Instance.ClearSlots();
        }

        public override void OnInteract()
        {
            base.OnInteract();

            if (!isInterfaceOpen)
            {
                StartCoroutine(OpenInterfaceAfterDelay());
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
