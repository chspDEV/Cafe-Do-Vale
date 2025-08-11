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

        public bool CanStoreProduct(BaseProduct product)
        {
            if (inventory == null) return false;
            if (!inventory.CanStorage()) return false;

            // Se não tem item definido, aceita qualquer
            if (item == null) return true;

            // Se tem item definido, deve ser compatível
            return item.productID == product.productID;
        }

        public bool ForceAddProduct(BaseProduct product, int quantity = 1)
        {
            if (inventory == null)
            {
                Debug.LogError($"[StorageArea] Inventory é null no armazém {areaID}!");
                return false;
            }

            try
            {
                // Se o armazém não tem item definido, definir agora
                if (item == null)
                {
                    item = product;
                    Debug.Log($"[StorageArea] Armazém {areaID} agora aceita: {product.productName}");
                }

                bool success = inventory.AddProductReturn(product, quantity);

                if (success)
                {
                    Debug.Log($"[StorageArea] ✓ {quantity}x {product.productName} adicionado ao armazém {areaID}");
                   
                }
                else
                {
                    Debug.LogWarning($"[StorageArea] Falha ao adicionar {product.productName} no armazém {areaID}");
                }

                return success;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StorageArea] Erro ao adicionar produto: {e.Message}");
                return false;
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
