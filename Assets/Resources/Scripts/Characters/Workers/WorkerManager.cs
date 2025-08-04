using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tcp4.Assets.Resources.Scripts.Systems.Areas;
using Tcp4.Assets.Resources.Scripts.Systems.Collect_Cook;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Tcp4.Assets.Resources.Scripts.Managers
{
    public class WorkerManager : Singleton<WorkerManager>
    {
        [Header("Worker Prefabs")]
        [SerializeField] private GameObject workerBasePrefab;
        [SerializeField] private List<GameObject> workerVisualModels;

        [Header("Scene References")]
        [SerializeField] private Transform workerSpawnPoint;
        [SerializeField] private Transform restArea;
        [SerializeField] private Transform homePosition;

        [Header("System Configuration")]
        [SerializeField] private float workerMoveSpeed = 3.5f;
        [SerializeField] private float defaultActionDuration = 2f;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        // --- GERENCIAMENTO DE TAREFAS ---
        private readonly List<WorkerTask> pendingTasks = new();
        private readonly List<WorkerTask> activeTasks = new();
        private readonly List<WorkerTask> completedTasks = new();

        // --- GERENCIAMENTO DE TRABALHADORES ---
        private readonly List<Worker> activeWorkers = new();
        private List<WorkerData> workerDataList = new();
        private int nextWorkerID = 1;

        // --- ARMAZENAMENTO DE ESTAÇÕES ---
        private readonly Dictionary<int, ProductionArea> productionStations = new();
        private readonly Dictionary<int, CreationArea> creationStations = new();
        private readonly Dictionary<int, StorageArea> storageStations = new();
        private readonly Dictionary<int, RefinementArea> refinementStations = new();

        // --- JOB SYSTEM ---
        private JobHandle workerJobHandle;
        private NativeArray<WorkerAction> lastFrameActions;
        private NativeArray<WorkerData> jobWorkerDataArray;

        // --- EVENTOS ---
        public event Action<WorkerData> OnWorkerHired;
        public event Action<int> OnWorkerFired;
        public event Action<WorkerTask> OnTaskCompleted;
        public event Action<WorkerTask> OnTaskFailed;

        #region Unity Methods
        public override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnWorkingHoursStart += OnWorkingHoursStart;
                TimeManager.Instance.OnWorkingHoursEnd += OnWorkingHoursEnd;
            }
        }

        private void Update()
        {
            if (activeWorkers.Count > 0)
            {
                AssignTasksToIdleWorkers();
                ProcessWorkerDecisions();
            }
            CleanupCompletedTasks();
        }

        private void LateUpdate()
        {
            if (workerJobHandle.IsCompleted)
            {
                workerJobHandle.Complete();
                ProcessJobResults();
            }
        }

        private void OnDestroy()
        {
            workerJobHandle.Complete();
            CleanupNativeArrays();
        }
        #endregion

        #region Station Registration
        public void RegisterProductionStation(int id, ProductionArea station)
        {
            if (!productionStations.ContainsKey(id)) productionStations.Add(id, station);
        }

        public void RegisterCreationStation(int id, CreationArea station)
        {
            if (!creationStations.ContainsKey(id)) creationStations.Add(id, station);
        }

        public void RegisterStorageStation(int id, StorageArea station)
        {
            if (!storageStations.ContainsKey(id)) storageStations.Add(id, station);
        }

        public void RegisterRefinementStation(int id, RefinementArea station)
        {
            if (!refinementStations.ContainsKey(id)) refinementStations.Add(id, station);
        }
        #endregion

        #region Task Management (com WorkerTaskFactory)
        public void CreateHarvestTask(int productionAreaID, BaseProduct product)
        {
            if (!productionStations.ContainsKey(productionAreaID))
            {
                Debug.LogWarning($"Tentativa de criar tarefa de colheita para estação não registrada: {productionAreaID}");
                return;
            }
            int storageID = FindAppropriateStorage(product);
            if (storageID == -1)
            {
                Debug.LogWarning($"Não foi possível encontrar armazém para o produto: {product.name}");
                return;
            }
            var task = WorkerTaskFactory.CreateHarvestTask(productionAreaID, storageID, product.productID);
            pendingTasks.Add(task);
            if (enableDebugLogs) Debug.Log($"Tarefa de colheita criada: {task.taskID}");
        }

        public void CreateRefineTask(BaseProduct rawProduct, BaseProduct refinedProduct)
        {
            int rawStorageID = FindStorageWithProduct(rawProduct.productID);
            int machineID = FindAvailableRefinementMachine();
            int refinedStorageID = FindAppropriateStorage(refinedProduct);

            if (rawStorageID == -1 || machineID == -1 || refinedStorageID == -1)
            {
                Debug.LogWarning("Recursos indisponíveis para criar tarefa de refinamento.");
                return;
            }
            var task = WorkerTaskFactory.CreateRefineTask(rawStorageID, machineID, refinedStorageID, rawProduct.productID, refinedProduct.productID);
            pendingTasks.Add(task);
            if (enableDebugLogs) Debug.Log($"Tarefa de refinamento criada: {task.taskID}");
        }

        public void CreateDrinkTask(Drink drink, int orderID)
        {
            // CORREÇÃO: Converte explicitamente o 'productID' de cada ingrediente para 'int'.
            var ingredientIDs = drink.requiredIngredients.Select(i => (int)i.productID).ToList();

            int coffeeStationID = FindAvailableCoffeeStation();
            int deliveryPointID = FindDeliveryPoint();

            if (coffeeStationID == -1 || deliveryPointID == -1 || !AreIngredientsAvailable(ingredientIDs))
            {
                Debug.LogWarning($"Recursos indisponíveis para criar tarefa de bebida: {drink.name}");
                return;
            }

            var task = WorkerTaskFactory.CreateDrinkTask(orderID, ingredientIDs, coffeeStationID, deliveryPointID, drink.productID);

            pendingTasks.Add(task);
            if (enableDebugLogs) Debug.Log($"Tarefa de bebida criada: {task.taskID}");
        }

        public void CompleteTask(int taskID)
        {
            int taskIndex = activeTasks.FindIndex(t => t.taskID == taskID);
            if (taskIndex != -1)
            {
                var task = activeTasks[taskIndex];
                task.status = TaskStatus.Completed;
                activeTasks.RemoveAt(taskIndex);
                completedTasks.Add(task);
                OnTaskCompleted?.Invoke(task);

                if (task.type == TaskType.Harvest)
                {
                    if (productionStations.TryGetValue(task.originID, out var area))
                    {
                        area.HarvestProductFromWorker(); // Novo método que você criará abaixo
                        TryStoreHarvestedItem(task.outputItemID);

                    }


                }

            }
        }

        public void FailTask(int taskID)
        {
            int taskIndex = activeTasks.FindIndex(t => t.taskID == taskID);
            if (taskIndex != -1)
            {
                var task = activeTasks[taskIndex];
                task.status = TaskStatus.Failed;
                activeTasks.RemoveAt(taskIndex);
                OnTaskFailed?.Invoke(task);
                StartCoroutine(RescheduleTaskAfterDelay(task, 5f));
            }
        }

        private IEnumerator RescheduleTaskAfterDelay(WorkerTask task, float delay)
        {
            yield return new WaitForSeconds(delay);
            task.status = TaskStatus.Pending;
            task.priority += 0.1f;
            pendingTasks.Add(task);
        }
        #endregion

        #region Worker Management (Com Save/Load)
        public void HireWorker(WorkerType type)
        {
            GameObject workerGO = Instantiate(workerBasePrefab, workerSpawnPoint.position, Quaternion.identity, transform);
            int newId = (workerDataList.Count > 0) ? workerDataList.Max(w => w.id) + 1 : 1;
            workerGO.name = $"Worker_{type}_{newId}";

            var data = new WorkerData
            {
                id = newId,
                homePosition = (float3)homePosition.position,
                type = type,
                currentState = WorkerState.Idle,
                actionDuration = defaultActionDuration,
                workDuration = 300f,
                restDuration = 60f,
                isActive = true,
                isHired = true,
                isWorkingTime = IsWorkingTime(),
                efficiency = UnityEngine.Random.Range(0.8f, 1.2f),
                currentPosition = workerSpawnPoint.position,
                currentTaskID = -1,
                carriedItems = new FixedList128Bytes<int>()
            };

            var workerComponent = workerGO.GetComponent<Worker>();
            workerComponent.Setup(data);

            activeWorkers.Add(workerGO.GetComponent<Worker>());

            // Aqui adiciona:
            if (workerVisualModels != null && workerVisualModels.Count > 0)
            {
                // Usa o índice com base no tipo, ou aleatório
                int modelIndex = Mathf.Clamp((int)type, 0, workerVisualModels.Count - 1);
                workerComponent.SetVisualModel(workerVisualModels[modelIndex]);
            }

            workerDataList.Add(data);
            OnWorkerHired?.Invoke(data);
        }

        public void FireWorker(int workerID)
        {
            var workerIndex = workerDataList.FindIndex(w => w.id == workerID);
            if (workerIndex != -1)
            {
                if (workerDataList[workerIndex].currentTaskID != -1)
                {
                    FailTask(workerDataList[workerIndex].currentTaskID);
                }
                Destroy(activeWorkers[workerIndex].gameObject);
                activeWorkers.RemoveAt(workerIndex);
                workerDataList.RemoveAt(workerIndex);
                OnWorkerFired?.Invoke(workerID);
            }
        }

        // MÉTODO RESTAURADO: Essencial para o SaveManager
        public void LoadWorker(WorkerData data)
        {
            GameObject workerGO = Instantiate(workerBasePrefab, data.homePosition, Quaternion.identity, transform);
            workerGO.name = $"Worker_{data.type}_{data.id}";
            Worker workerComponent = workerGO.GetComponent<Worker>();

            workerComponent.Setup(data);

            if (workerVisualModels != null && workerVisualModels.Count > 0)
            {
                // Usa o índice com base no tipo, ou aleatório
                int modelIndex = Mathf.Clamp((int)data.type, 0, workerVisualModels.Count - 1);
                workerComponent.SetVisualModel(workerVisualModels[modelIndex]);
            }

            workerDataList.Add(data);
            activeWorkers.Add(workerComponent);

            workerComponent.SetWorkingAnimation(data.currentState == WorkerState.Working);
        }

        // MÉTODO RESTAURADO: Essencial para o SaveManager
        public List<WorkerData> GetHiredWorkers()
        {
            return workerDataList.Where(w => w.isHired).ToList();
        }

        // MÉTODO RESTAURADO: Essencial para o SaveManager
        public void FireAllWorkers()
        {
            var workerIdsToFire = workerDataList.Select(w => w.id).ToList();
            foreach (int id in workerIdsToFire)
            {
                FireWorker(id);
            }
        }
        #endregion

        #region Job System & State Logic
        private void AssignTasksToIdleWorkers()
        {
            if (pendingTasks.Count == 0) return;
            pendingTasks.Sort((a, b) => b.priority.CompareTo(a.priority));
            var assignedTaskIndices = new HashSet<int>();

            for (int i = 0; i < workerDataList.Count; i++)
            {
                var worker = workerDataList[i];
                if (worker.currentState != WorkerState.Idle || !worker.isWorkingTime) continue;
                for (int j = 0; j < pendingTasks.Count; j++)
                {
                    if (assignedTaskIndices.Contains(j)) continue;
                    var task = pendingTasks[j];
                    if (task.requiredWorkerType == worker.type)
                    {
                        int originId = task.originID;
                        if (task.type == TaskType.CreateDrink)
                        {
                            // CORREÇÃO: Converte a FixedList para uma List<int> manualmente.
                            var ingredientsList = new List<int>();
                            foreach (var ingredient in task.requiredIngredients)
                            {
                                ingredientsList.Add(ingredient);
                            }
                            originId = FindStorageWithIngredients(ingredientsList);
                        }

                        if (originId != -1 && TryGetStationPosition(originId, out float3 targetPos))
                        {
                            worker.currentTaskID = task.taskID;
                            worker.currentState = WorkerState.MovingToOrigin;
                            worker.moveTarget = targetPos;
                            workerDataList[i] = worker;
                            activeTasks.Add(task);
                            assignedTaskIndices.Add(j);
                            break;
                        }
                    }
                }
            }

            if (assignedTaskIndices.Count > 0)
            {
                foreach (var index in assignedTaskIndices.OrderByDescending(idx => idx))
                {
                    pendingTasks.RemoveAt(index);
                }
            }
        }

        private void ProcessWorkerDecisions()
        {
            if (activeWorkers.Count == 0) return;

            for (int i = 0; i < workerDataList.Count; i++)
            {
                workerDataList[i] = UpdateCurrentPosition(workerDataList[i], activeWorkers[i].transform.position);
            }

            jobWorkerDataArray = new NativeArray<WorkerData>(workerDataList.ToArray(), Allocator.TempJob);

            lastFrameActions = new NativeArray<WorkerAction>(activeWorkers.Count, Allocator.TempJob);
            var activeTasksMap = new NativeHashMap<int, WorkerTask>(activeTasks.Count, Allocator.TempJob);
            foreach (var task in activeTasks) activeTasksMap.TryAdd(task.taskID, task);

            var job = new WorkerDecisionJob
            {
                workerDataArray = jobWorkerDataArray,
                ActiveTasks = activeTasksMap,
                workerActions = lastFrameActions,
                stationPositions = CreateStationPositionsMap(),
                allCarriedItems = new NativeArray<FixedList128Bytes<int>>(workerDataList.Select(w => w.carriedItems).ToArray(), Allocator.TempJob),
                deltaTime = Time.deltaTime,
                isWorkingTime = IsWorkingTime(),
            };

            workerJobHandle = job.Schedule(activeWorkers.Count, 1);
            JobHandle.ScheduleBatchedJobs();
        }

        private void ProcessJobResults()
        {
            if (!jobWorkerDataArray.IsCreated) return; // VERIFICA SE OS DADOS DO JOB FORAM CRIADOS.

            workerDataList = new List<WorkerData>(jobWorkerDataArray.ToArray());
            for (int i = 0; i < activeWorkers.Count; i++)
            {
                activeWorkers[i].UpdateWorkerData(workerDataList[i]);
                ProcessIndividualWorkerAction(activeWorkers[i], workerDataList[i], lastFrameActions[i]);
            }
            CleanupNativeArrays();
        }

        private WorkerData UpdateCurrentPosition(WorkerData data, Vector3 position)
        {
            data.currentPosition = position;
            return data;
        }

        private void ProcessIndividualWorkerAction(Worker worker, WorkerData data, WorkerAction action)
        {
            NavMeshAgent agent = worker.GetComponent<NavMeshAgent>();
            if (agent == null) return;
            switch (action)
            {
                case WorkerAction.MoveToTarget:
                    if (!agent.hasPath || Vector3.Distance(agent.destination, (Vector3)data.moveTarget) > 0.1f)
                    {
                        agent.SetDestination(data.moveTarget);
                    }
                    worker.SetWorkingAnimation(false);
                    break;
                case WorkerAction.CollectItem:
                case WorkerAction.WorkAtStation:
                case WorkerAction.DeliverItem:
                    agent.ResetPath();
                    worker.SetWorkingAnimation(true);
                    break;
                case WorkerAction.TaskCompleted:
                    CompleteTask(data.currentTaskID);
                    break;
                case WorkerAction.TaskFailed:
                    FailTask(data.currentTaskID);
                    break;
            }
        }
        #endregion

        #region Helper Methods
        private NativeHashMap<int, float3> CreateStationPositionsMap()
        {
            var capacity = productionStations.Count + creationStations.Count + storageStations.Count + refinementStations.Count;
            var map = new NativeHashMap<int, float3>(Mathf.Max(1, capacity), Allocator.TempJob);
            foreach (var s in productionStations) map.TryAdd(s.Key, s.Value.transform.position);
            foreach (var s in creationStations) map.TryAdd(s.Key, s.Value.transform.position);
            foreach (var s in storageStations) map.TryAdd(s.Key, s.Value.transform.position);
            foreach (var s in refinementStations) map.TryAdd(s.Key, s.Value.transform.position);
            return map;
        }

        public bool TryGetStationPosition(int stationId, out float3 position)
        {
            position = float3.zero;
            if (productionStations.TryGetValue(stationId, out var p)) { position = p.transform.position; return true; }
            if (creationStations.TryGetValue(stationId, out var c)) { position = c.transform.position; return true; }
            if (storageStations.TryGetValue(stationId, out var s)) { position = s.transform.position; return true; }
            if (refinementStations.TryGetValue(stationId, out var r)) { position = r.transform.position; return true; }
            return false;
        }

        private void TryStoreHarvestedItem(int productID, int quantity = 1)
        {
            StorageArea fallback = null;

            foreach (var storagePair in storageStations)
            {
                var storage = storagePair.Value;
                if (storage.inventory != null && storage.inventory.CanStorage())
                {
                    // Verifica compatibilidade
                    if (storage.item != null && storage.item.productID == productID)
                    {
                        storage.inventory.AddProduct(ProductDatabase.GetRawProductByID(productID), quantity);
                        if (enableDebugLogs) Debug.Log($"[Storage] Produto {productID} armazenado em armazém compatível {storage.areaID}");
                        return;
                    }

                    // Salva um armazém qualquer com espaço como fallback
                    if (fallback == null)
                        fallback = storage;
                }
            }

            // Se não encontrar armazém compatível, usar qualquer um disponível
            if (fallback != null)
            {
                fallback.inventory.AddProduct(ProductDatabase.GetRawProductByID(productID), quantity);
                if (enableDebugLogs) Debug.LogWarning($"[Storage] Produto {productID} armazenado no fallback {fallback.areaID} (incompatível)");
            }
            else
            {
                Debug.LogWarning($"[Storage] Nenhum armazém com espaço disponível para o produto {productID}");
            }
        }



        private int FindAppropriateStorage(BaseProduct product)
        {
            foreach (var pair in storageStations)
            {
                var storage = pair.Value;
                if (storage.item != null && storage.item.productID == product.productID && storage.inventory.CanStorage())
                {
                    return pair.Key;
                }
            }

            // Fallback: qualquer com espaço
            foreach (var pair in storageStations)
            {
                if (pair.Value.inventory.CanStorage())
                {
                    return pair.Key;
                }
            }

            return -1;
        }

        private int FindStorageWithProduct(int productID) => storageStations.Keys.FirstOrDefault();
        private int FindAvailableRefinementMachine() => refinementStations.Keys.FirstOrDefault();
        private int FindAvailableCoffeeStation() => creationStations.Keys.FirstOrDefault();
        private int FindDeliveryPoint() => storageStations.Keys.FirstOrDefault();
        private bool AreIngredientsAvailable(List<int> ingredientIDs) => true;
        private int FindStorageWithIngredients(List<int> ingredientIDs) => storageStations.Keys.FirstOrDefault();
        private bool IsWorkingTime() => TimeManager.Instance?.IsWorkingTime() ?? true;
        private void CleanupCompletedTasks() { if (completedTasks.Count > 100) completedTasks.RemoveRange(0, 50); }
        private void CleanupNativeArrays()
        {
            if (jobWorkerDataArray.IsCreated) jobWorkerDataArray.Dispose();
            if (lastFrameActions.IsCreated) lastFrameActions.Dispose();
            // A NativeArray criada para 'allCarriedItems' também é temporária (TempJob) e não precisa ser limpa manualmente aqui.
        }
        #endregion

        #region Event Handlers
        private void OnWorkingHoursStart()
        {
            for (int i = 0; i < workerDataList.Count; i++)
            {
                var data = workerDataList[i];
                data.isWorkingTime = true;
                if (data.currentState == WorkerState.OffDuty) data.currentState = WorkerState.Idle;
                workerDataList[i] = data;
            }
        }
        private void OnWorkingHoursEnd() { }
        #endregion

        #region Public API
        public List<WorkerData> GetActiveWorkers() => workerDataList;
        public List<WorkerTask> GetPendingTasks() => pendingTasks;
        public List<WorkerTask> GetActiveTasks() => activeTasks;
        public WorkerData? GetWorkerByID(int workerID)
        {
            int index = workerDataList.FindIndex(w => w.id == workerID);
            return index != -1 ? workerDataList[index] : null;
        }
        #endregion
    }
}