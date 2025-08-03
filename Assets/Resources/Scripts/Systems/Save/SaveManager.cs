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
        [Header("Refer�ncias")]
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
            SaveQuestData(data); // NOVO: Salva dados das quests

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
            LoadQuestData(data); // NOVO: Carrega dados das quests

            Debug.Log("Todos os dados foram carregados.");
            OnGameDataLoaded?.Invoke();
        }

        #region Quest Save
        private void SaveQuestData(GameData data)
        {
            if (QuestManager.Instance == null)
            {
                Debug.LogWarning("QuestManager.Instance is null, skipping quest data save.");
                return;
            }

            // Salva quests do tutorial
            var tutorialMissions = QuestManager.Instance.GetTutorialMissions();
            if (tutorialMissions != null)
            {
                foreach (var quest in tutorialMissions)
                {
                    var savedQuest = new SavedQuest
                    {
                        questID = quest.questID,
                        isCompleted = quest.isCompleted,
                        isStarted = quest.isStarted
                    };

                    // Salva o progresso dos steps
                    if (quest.steps != null)
                    {
                        for (int i = 0; i < quest.steps.Count; i++)
                        {
                            savedQuest.steps.Add(new SavedQuestStep
                            {
                                stepIndex = i,
                                isCompleted = quest.steps[i].isCompleted
                            });
                        }
                    }

                    data.questData.Add(savedQuest);
                }
            }

            Debug.Log($"Saved {data.questData.Count} quests to save data.");
        }

        private void LoadQuestData(GameData data)
        {
            if (QuestManager.Instance == null)
            {
                Debug.LogWarning("QuestManager.Instance is null, skipping quest data load.");
                return;
            }

            var tutorialMissions = QuestManager.Instance.GetTutorialMissions();
            if (tutorialMissions == null || data.questData == null)
            {
                Debug.LogWarning("No quest data to load or tutorialMissions is null.");
                return;
            }

            foreach (var savedQuest in data.questData)
            {
                var quest = tutorialMissions.FirstOrDefault(q => q.questID == savedQuest.questID);
                if (quest != null)
                {
                    quest.isCompleted = savedQuest.isCompleted;
                    quest.isStarted = savedQuest.isStarted;

                    // Restaura o progresso dos steps
                    if (quest.steps != null && savedQuest.steps != null)
                    {
                        foreach (var savedStep in savedQuest.steps)
                        {
                            if (savedStep.stepIndex >= 0 && savedStep.stepIndex < quest.steps.Count)
                            {
                                quest.steps[savedStep.stepIndex].isCompleted = savedStep.isCompleted;
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Quest Load: Quest '{savedQuest.questID}' not found in tutorialMissions.");
                }
            }

            Debug.Log($"Loaded {data.questData.Count} quests from save data.");
        }
        #endregion

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
                playerRb.linearVelocity = Vector3.zero;
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
                    Debug.LogWarning($"Storage Load: Storage com ID '{savedStorage.storageID}' n�o encontrado.");
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
                        Debug.LogWarning($"Storage Load: Produto '{savedItem.productName}' n�o encontrado nos Resources.");
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

            Debug.LogWarning($"Produto '{name}' n�o encontrado nas pastas.");
            return null;
        }

        #endregion

        #region Player Inventory Save
        private void SavePlayerInventory(GameData data)
        {
            Inventory playerInventory = player.GetComponent<Inventory>();
            if (playerInventory == null)
            {
                Debug.LogError("SaveManager n�o encontrou o componente Inventory no jogador!");
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
                Debug.LogError("SaveManager n�o encontrou o componente Inventory no jogador!");
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
                    Debug.LogWarning($"Player Inventory Load: Produto '{savedItem.productName}' n�o encontrado.");
                }
            }
        }
        #endregion
    }
}
