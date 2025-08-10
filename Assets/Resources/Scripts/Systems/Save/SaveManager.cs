using System.IO;
using Tcp4.Assets.Resources.Scripts.Managers;
using Tcp4.Assets.Resources.Scripts.Systems.Collect_Cook;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Tcp4
{
    public class SaveManager : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] private ShopManager shopManager;
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private PlayerMovement player;
        [SerializeField] private WorkerManager workerManager;

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
            SaveQuestData(data);
            SaveWorkerData(data);

            // NOVO: Salva sistema de desbloqueio
            SaveUnlockData(data);

            SaveSystem.SaveGame(data);
        }

        public void Start()
        {
#if UNITY_EDITOR
            return;
#endif
            Load();
        }

        public void Load()
        {
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
            LoadQuestData(data);
            LoadWorkerData(data);

            // NOVO: Carrega sistema de desbloqueio
            LoadUnlockData(data);

            Debug.Log("Todos os dados foram carregados.");
            OnGameDataLoaded?.Invoke();
        }

        #region Unlock System Save
        private void SaveUnlockData(GameData data)
        {
            if (UnlockManager.Instance == null) return;

            data.currentReputationLevel = UnlockManager.Instance.CurrentReputationLevel;
            data.productionsUnlocked = UnlockManager.Instance.config.unlockableProductions.Select(u => u.isUnlocked).ToList();
            data.drinksUnlocked = UnlockManager.Instance.config.unlockableDrinks.Select(u => u.isUnlocked).ToList();
            data.cupsUnlocked = UnlockManager.Instance.config.unlockableCups.Select(u => u.isUnlocked).ToList();
        }

        private void LoadUnlockData(GameData data)
        {
            if (UnlockManager.Instance == null) return;

            // Só restaura se existir dado salvo
            if (data.productionsUnlocked != null && data.productionsUnlocked.Count > 0)
                RestoreUnlockList(UnlockManager.Instance.config.unlockableProductions, data.productionsUnlocked);

            if (data.drinksUnlocked != null && data.drinksUnlocked.Count > 0)
                RestoreUnlockList(UnlockManager.Instance.config.unlockableDrinks, data.drinksUnlocked);

            if (data.cupsUnlocked != null && data.cupsUnlocked.Count > 0)
                RestoreUnlockList(UnlockManager.Instance.config.unlockableCups, data.cupsUnlocked);

            UnlockManager.Instance.SetReputation(data.currentReputationLevel);
        }

        private void RestoreUnlockList<T>(List<UnlockableItem<T>> target, List<bool> saved)
        {
            for (int i = 0; i < target.Count && i < saved.Count; i++)
                target[i].isUnlocked = saved[i];
        }
        #endregion

        #region Quest Save
        private void SaveQuestData(GameData data)
        {
            if (QuestManager.Instance == null) return;
            var tutorialMissions = QuestManager.Instance.GetTutorialMissions();
            if (tutorialMissions == null) return;

            foreach (var quest in tutorialMissions)
            {
                var savedQuest = new SavedQuest
                {
                    questID = quest.questID,
                    isCompleted = quest.isCompleted,
                    isStarted = quest.isStarted,
                    steps = new List<SavedQuestStep>()
                };
                for (int i = 0; i < quest.steps.Count; i++)
                {
                    savedQuest.steps.Add(new SavedQuestStep { stepIndex = i, isCompleted = quest.steps[i].isCompleted });
                }
                data.questData.Add(savedQuest);
            }
        }

        private void LoadQuestData(GameData data)
        {
            if (QuestManager.Instance == null || data.questData == null) return;
            var tutorialMissions = QuestManager.Instance.GetTutorialMissions();
            if (tutorialMissions == null) return;

            foreach (var savedQuest in data.questData)
            {
                var quest = tutorialMissions.FirstOrDefault(q => q.questID == savedQuest.questID);
                if (quest != null)
                {
                    quest.isCompleted = savedQuest.isCompleted;
                    quest.isStarted = savedQuest.isStarted;
                    foreach (var savedStep in savedQuest.steps)
                    {
                        if (savedStep.stepIndex < quest.steps.Count)
                        {
                            quest.steps[savedStep.stepIndex].isCompleted = savedStep.isCompleted;
                        }
                    }
                }
            }
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
            StorageArea[] storages = FindObjectsByType<StorageArea>(FindObjectsSortMode.None);

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
            StorageArea[] allStorages = FindObjectsByType<StorageArea>(FindObjectsSortMode.None);
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
            foreach (var storage in FindObjectsByType<StorageArea>(FindObjectsSortMode.None))
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

        #region Workers
        private void SaveWorkerData(GameData data)
        {
            if (workerManager == null) return;

            data.hiredWorkersData.Clear();
            List<WorkerData> hiredWorkers = workerManager.GetHiredWorkers();

            foreach (var worker in hiredWorkers)
            {
                SavedWorkerData savedWorker = new SavedWorkerData
                {
                    workerID = worker.id, // Importante salvar o ID!
                    //workerName = worker.workerName,
                    type = worker.type,
                    efficiency = worker.efficiency,
                    dailyCost = worker.dailyCost,
                    pauseChance = worker.pauseChance,
                    workDuration = worker.workDuration,
                    restDuration = worker.restDuration,
                    homePosition = new SerializableVector3(worker.homePosition)
                };
                data.hiredWorkersData.Add(savedWorker);
            }
        }

        private void LoadWorkerData(GameData data)
        {
            if (workerManager == null || data.hiredWorkersData == null) return;

            workerManager.FireAllWorkers();

            foreach (var savedWorker in data.hiredWorkersData)
            {
                WorkerData workerData = new WorkerData
                {
                    id = savedWorker.workerID,
                    //workerName = savedWorker.workerName,
                    type = savedWorker.type,
                    efficiency = savedWorker.efficiency,
                    dailyCost = savedWorker.dailyCost,
                    pauseChance = savedWorker.pauseChance,
                    workDuration = savedWorker.workDuration,
                    restDuration = savedWorker.restDuration,
                    homePosition = savedWorker.homePosition.ToVector3(),
                    isHired = true,
                    isActive = true,
                    currentState = WorkerState.Idle
                };

                workerManager.LoadWorker(workerData);
            }
        }
        #endregion
    }
}
