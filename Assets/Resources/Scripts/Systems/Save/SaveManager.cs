using System.IO;
using Tcp4.Assets.Resources.Scripts.Managers;
using Tcp4.Assets.Resources.Scripts.Systems.Collect_Cook;
using UnityEngine;
using System.Linq;
using System.Collections;

namespace Tcp4
{
    public class SaveManager : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] private ShopManager shopManager;
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private PlayerMovement player;

        public static event System.Action OnGameDataLoaded;

        public void Save()
        {
            GameData data = new GameData
            {
                money = shopManager.GetMoney(),
                stars = shopManager.GetStars(),
                gameTime = timeManager.CurrentHour,
                playerPosition = new SerializableVector3(player.transform.position)
            };

            SaveSeedInventory(data);
            SaveStorageData(data);
            SavePlayerInventory(data);

            SaveSystem.SaveGame(data);
        }

        public void Load()
        {
            // fiz uma corrotina pq tava atropelando outros scripts antes de carregar
            StartCoroutine(LoadGame_Coroutine());
        }

        private IEnumerator LoadGame_Coroutine()
        {
            yield return new WaitForEndOfFrame();

            GameData data = SaveSystem.LoadGame();
            if (data == null)
            {
                Debug.LogWarning("Nenhum arquivo de save encontrado.");
                yield break;
            }

            shopManager.SetMoney(data.money);
            shopManager.SetStars(data.stars);
            timeManager.SetHour(data.gameTime);

            StartCoroutine(SetPlayerPosition_Coroutine(data.playerPosition.ToVector3()));

            LoadSeedInventory(data);
            LoadStorageData(data);
            LoadPlayerInventory(data);

            Debug.Log("Todos os dados foram carregados.");
            OnGameDataLoaded?.Invoke();
        }

        #region Player Position Save
        private IEnumerator SetPlayerPosition_Coroutine(Vector3 position)
        {
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            Rigidbody playerRb = player.GetComponent<Rigidbody>();

            if (playerMovement != null)
            {
                playerMovement.ResetState();
            }

            if (playerRb != null)
            {
                playerRb.isKinematic = true;
            }

            player.transform.position = position;

            yield return new WaitForEndOfFrame();

            if (playerRb != null)
            {
                playerRb.isKinematic = false;
                playerRb.velocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
            }
        }
        #endregion

        #region Seed Inventory Save
        private void SaveSeedInventory(GameData data)
        {
            foreach (var pair in SeedManager.Instance.GetInventory)
            {
                var production = pair.Key;
                int quantity = pair.Value;

                data.seedInventoryItems.Add(new SavedSeed
                {
                    productionName = production.name,
                    quantity = quantity
                });
            }
        }

        private void LoadSeedInventory(GameData data)
        {
            var inventory = SeedManager.Instance.GetInventory;
            inventory.Clear();

            foreach (var savedSeed in data.seedInventoryItems)
            {
                Production prod = FindProductionByName(savedSeed.productionName);
                if (prod != null)
                {
                    inventory.Add(prod, savedSeed.quantity);
                }
                else
                {
                    Debug.LogWarning($"Seed Load: Production '{savedSeed.productionName}' not found.");
                }
            }

            UIManager.Instance.UpdateSeedInventoryView();
        }

        private Production FindProductionByName(string name)
        {
            foreach (var seed in SeedManager.Instance.GetAllSeeds)
            {
                if (seed.targetProduction != null && seed.targetProduction.name == name)
                {
                    return seed.targetProduction;
                }
            }

            return null;
        }
        #endregion

        #region Storage Area Save
        private void SaveStorageData(GameData data)
        {
            StorageArea[] storages = FindObjectsOfType<StorageArea>();

            foreach (var storage in storages)
            {
                if (string.IsNullOrEmpty(storage.storageID)) continue;

                var productList = storage.inventory.GetInventory();

                if (productList.Count == 0) continue;

                var groupedItems = productList
                    .GroupBy(product => product)
                    .ToDictionary(group => group.Key, group => group.Count());

                SavedStorageData storageData = new SavedStorageData
                {
                    storageID = storage.storageID
                };

                foreach (var itemPair in groupedItems)
                {
                    storageData.items.Add(new SavedStorageItem
                    {
                        productName = itemPair.Key.productName,
                        quantity = itemPair.Value
                    });
                }

                data.storageAreaItems.Add(storageData);
            }
        }

        private void LoadStorageData(GameData data)
        {
            StorageArea[] allStorages = FindObjectsOfType<StorageArea>();
            foreach (var storage in allStorages)
            {
                storage.inventory.Clear();
            }

            foreach (var savedStorage in data.storageAreaItems)
            {
                StorageArea targetStorage = FindStorageByID(savedStorage.storageID);
                if (targetStorage == null)
                {
                    Debug.LogWarning($"Storage Load: Storage com ID '{savedStorage.storageID}' não encontrado.");
                    continue;
                }

                foreach (var savedItem in savedStorage.items)
                {
                    BaseProduct product = FindProductByName(savedItem.productName);
                    if (product != null)
                    {
                        targetStorage.inventory.AddProduct(product, savedItem.quantity);
                    }
                    else
                    {
                        Debug.LogWarning($"Storage Load: Produto '{savedItem.productName}' não encontrado nos Resources.");
                    }
                }
            }
        }

        private StorageArea FindStorageByID(string id)
        {
            foreach (var storage in FindObjectsOfType<StorageArea>())
            {
                if (storage.storageID == id)
                {
                    return storage;
                }
            }
            return null;
        }

        private BaseProduct FindProductByName(string name)
        {
            var product = UnityEngine.Resources.Load<BaseProduct>($"Database/RawProductSO/{name}");
            if (product != null) return product;

            product = UnityEngine.Resources.Load<BaseProduct>($"Database/IngredientsSO/{name}");
            if (product != null) return product;

            Debug.LogWarning($"Produto '{name}' não encontrado nas pastas.");
            return null;
        }

        #endregion

        #region Player Inventory Save
        private void SavePlayerInventory(GameData data)
        {
            Inventory playerInventory = player.GetComponent<Inventory>();
            if (playerInventory == null)
            {
                Debug.LogError("SaveManager não encontrou o componente Inventory no jogador!");
                return;
            }

            var productList = playerInventory.GetInventory();
            var groupedItems = productList
                .GroupBy(product => product)
                .ToDictionary(group => group.Key, group => group.Count());

            foreach (var itemPair in groupedItems)
            {
                data.playerBagItems.Add(new SavedStorageItem
                {
                    productName = itemPair.Key.productName,
                    quantity = itemPair.Value
                });
            }
        }

        private void LoadPlayerInventory(GameData data)
        {
            Inventory playerInventory = player.GetComponent<Inventory>();
            if (playerInventory == null)
            {
                Debug.LogError("SaveManager não encontrou o componente Inventory no jogador!");
                return;
            }

            playerInventory.Clear();

            foreach (var savedItem in data.playerBagItems)
            {
                BaseProduct product = FindProductByName(savedItem.productName);
                if (product != null)
                {
                    playerInventory.AddProduct(product, savedItem.quantity);
                }
                else
                {
                    Debug.LogWarning($"Player Inventory Load: Produto '{savedItem.productName}' não encontrado.");
                }
            }
        }
        #endregion
    }
}
