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
        private readonly Dictionary<int, WorkerTask> activeTasks = new();
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
            if (enableDebugLogs) Debug.Log($"[WorkerManager] Tentando criar tarefa de colheita para área {productionAreaID}");

            if (!productionStations.ContainsKey(productionAreaID))
            {
                Debug.LogWarning($"[WorkerManager] Área de produção {productionAreaID} não registrada");
                return;
            }

            var productionArea = productionStations[productionAreaID];

            // Verificar se há produto para colher
            if (!productionArea.HasHarvestableProduct())
            {
                if (enableDebugLogs) Debug.Log($"[WorkerManager] Área {productionAreaID} não tem produto pronto para colheita");
                return;
            }

            int storageID = FindAppropriateStorage(product);
            if (storageID == -1)
            {
                Debug.LogWarning($"[WorkerManager] Nenhum armazém disponível para {product.productName}");
                return;
            }

            var task = WorkerTaskFactory.CreateHarvestTask(productionAreaID, storageID, product.productID);
            pendingTasks.Add(task);

            if (enableDebugLogs) Debug.Log($"[WorkerManager] ✓ Tarefa de colheita criada: {task.taskID} para {product.productName}");
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
            if (activeTasks.Remove(taskID, out var task))
            {
                task.status = TaskStatus.Completed;
                completedTasks.Add(task);

                // Liberar reserva da área de produção
                if (task.type == TaskType.Harvest && productionStations.TryGetValue(task.originID, out var area))
                {
                    area.ReleaseReservation();
                    if (enableDebugLogs) Debug.Log($"[WorkerManager] Reserva da área {area.areaID} liberada após completar tarefa {taskID}");
                }

                if (enableDebugLogs) Debug.Log($"[WorkerManager] ✓ TAREFA COMPLETA: {taskID} tipo {task.type}");
                OnTaskCompleted?.Invoke(task);
            }
            else
            {
                Debug.LogWarning($"[WorkerManager] Tarefa {taskID} não foi encontrada nas ativas ao tentar completar!");
            }
        }

        private void ProcessHarvestTaskCompletion(WorkerTask task)
        {
            if (enableDebugLogs) Debug.Log($"[WorkerManager] Processando colheita - Tarefa {task.taskID}");

            // Verificar se a área de produção existe
            if (!productionStations.TryGetValue(task.originID, out var productionArea))
            {
                Debug.LogError($"[WorkerManager] Área de produção {task.originID} não encontrada!");
                return;
            }

            // Obter o produto antes de colher
            BaseProduct harvestedProduct = productionArea.GetCurrentProduct();
            if (harvestedProduct == null)
            {
                Debug.LogError($"[WorkerManager] Produto nulo na área {task.originID}!");
                return;
            }

            // Executar a colheita
            bool harvestSuccess = productionArea.HarvestProductFromWorker();

            if (!harvestSuccess)
            {
                Debug.LogError($"[WorkerManager] Falha na colheita da área {task.originID}!");
                return;
            }

            if (enableDebugLogs) Debug.Log($"[WorkerManager] ✓ Colheita bem-sucedida: {harvestedProduct.productName}");

            // Armazenar o produto colhido
            TryStoreHarvestedItem(harvestedProduct, task.destinationID, 1);
        }

        // NOVO: Processar outras tarefas (placeholder)
        private void ProcessRefineTaskCompletion(WorkerTask task)
        {
            if (enableDebugLogs) Debug.Log($"[WorkerManager] Processando refinamento - Tarefa {task.taskID}");
            // Implementar lógica de refinamento
        }

        private void ProcessDrinkTaskCompletion(WorkerTask task)
        {
            if (enableDebugLogs) Debug.Log($"[WorkerManager] Processando bebida - Tarefa {task.taskID}");
            // Implementar lógica de criação de bebida
        }

        // Em WorkerManager.cs
        public void FailTask(int taskID)
        {
            if (activeTasks.Remove(taskID, out var task))
            {
                task.status = TaskStatus.Failed;

                // Liberar reserva da área de produção
                if (task.type == TaskType.Harvest && productionStations.TryGetValue(task.originID, out var area))
                {
                    area.ReleaseReservation();
                    if (enableDebugLogs) Debug.Log($"[WorkerManager] Reserva da área {area.areaID} liberada após falha da tarefa {taskID}");
                }

                OnTaskFailed?.Invoke(task);

                if (task.type != TaskType.Harvest)
                {
                    StartCoroutine(RescheduleTaskAfterDelay(task, 5f));
                }
                else
                {
                    if (enableDebugLogs) Debug.Log($"[WorkerManager] Tarefa de colheita {task.taskID} falhou e foi descartada.");
                }
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
                            var ingredientsList = new List<int>();
                            foreach (var ingredient in task.requiredIngredients)
                            {
                                ingredientsList.Add(ingredient);
                            }
                            originId = FindStorageWithIngredients(ingredientsList);
                        }

                        if (originId != -1 && TryGetStationPosition(originId, out float3 targetPos))
                        {
                            // VERIFICAÇÃO ADICIONAL: Para tarefas de colheita, verificar se a área ainda tem produto
                            if (task.type == TaskType.Harvest && productionStations.TryGetValue(task.originID, out var productionArea))
                            {
                                if (!productionArea.HasHarvestableProduct())
                                {
                                    if (enableDebugLogs) Debug.Log($"[WorkerManager] Tarefa {task.taskID} cancelada - área {task.originID} não tem produto");
                                    assignedTaskIndices.Add(j); // Marcar para remoção
                                    continue;
                                }

                                // Reservar APENAS quando atribuir a tarefa ao worker
                                productionArea.ReserveForWorker();
                                if (enableDebugLogs) Debug.Log($"[WorkerManager] Área {productionArea.areaID} reservada para Worker {worker.id}");
                            }

                            worker.currentTaskID = task.taskID;
                            worker.currentState = WorkerState.MovingToOrigin;
                            worker.moveTarget = targetPos;
                            workerDataList[i] = worker;
                            activeTasks.Add(task.taskID, task);
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

            foreach (KeyValuePair<int, WorkerTask> taskPair in activeTasks)
            {
                activeTasksMap.TryAdd(taskPair.Key, taskPair.Value);
            }

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

        // Em WorkerManager.cs
        // Em WorkerManager.cs
        private void ProcessJobResults()
        {
            if (!jobWorkerDataArray.IsCreated) return;

            workerDataList = new List<WorkerData>(jobWorkerDataArray.ToArray());

            for (int i = 0; i < activeWorkers.Count; i++)
            {
                // Pega a struct de dados atual para este trabalhador.
                var data = workerDataList[i];

                // Atualiza a visualização do trabalhador (como o painel de debug na tela).
                activeWorkers[i].UpdateWorkerData(data);

                // Processa a ação, passando a struct 'data' POR REFERÊNCIA para que possa ser modificada.
                ProcessIndividualWorkerAction(activeWorkers[i], ref data, lastFrameActions[i]);

                // Salva a struct 'data' (agora potencialmente modificada) de volta na lista principal.
                // Este passo é crucial para persistir a mudança para o próximo frame.
                workerDataList[i] = data;
            }

            CleanupNativeArrays();
        }

        private WorkerData UpdateCurrentPosition(WorkerData data, Vector3 position)
        {
            data.currentPosition = position;
            return data;
        }

        // Em WorkerManager.cs
        // Em WorkerManager.cs
        private void ProcessIndividualWorkerAction(Worker worker, ref WorkerData data, WorkerAction action)
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
                    agent.ResetPath();
                    worker.SetWorkingAnimation(true);
                    ExecuteCollectAction(ref data);
                    break;
                case WorkerAction.WorkAtStation:
                case WorkerAction.DeliverItem:
                    agent.ResetPath();
                    worker.SetWorkingAnimation(true);
                    ExecuteDeliverAction(ref data);
                    break;
                case WorkerAction.TaskCompleted:
                    CompleteTask(data.currentTaskID);
                    break;
                case WorkerAction.TaskFailed:
                    FailTask(data.currentTaskID);
                    break;
            }
        }

        // Correção no WorkerManager.cs - ExecuteCollectAction

        private void ExecuteCollectAction(ref WorkerData data)
        {
            // VERIFICAÇÃO CRÍTICA: Se o worker já está carregando item, não executar novamente
            if (data.isCarryingItem)
            {
                if (enableDebugLogs) Debug.Log($"[WorkerManager] Worker {data.id} já está carregando item - pulando ExecuteCollectAction");
                return;
            }

            if (activeTasks.TryGetValue(data.currentTaskID, out var task))
            {
                if (task.type == TaskType.Harvest)
                {
                    if (productionStations.TryGetValue(task.originID, out var productionArea))
                    {
                        // Verificação adicional de segurança
                        if (!productionArea.HasHarvestableProduct())
                        {
                            if (enableDebugLogs) Debug.LogWarning($"[WorkerManager] Área {task.originID} não tem produto para colher - Worker {data.id}");
                            return; // Não falha, apenas retorna
                        }

                        BaseProduct productToHarvest = productionArea.GetCurrentProduct();
                        if (productToHarvest != null)
                        {
                            // Tentar colher o produto
                            bool harvestSuccess = productionArea.HarvestProductFromWorker();

                            if (harvestSuccess)
                            {
                                // Marcar o worker como carregando o item IMEDIATAMENTE
                                data.carriedItemID = productToHarvest.productID;
                                data.isCarryingItem = true;
                                data.inventoryCount = 1;

                                if (enableDebugLogs) Debug.Log($"[WorkerManager] ✓ Worker {data.id} coletou {productToHarvest.productName} da área {task.originID}");
                            }
                            else
                            {
                                if (enableDebugLogs) Debug.LogWarning($"[WorkerManager] Worker {data.id} falhou ao coletar da área {task.originID} - HarvestProductFromWorker retornou false");
                            }
                        }
                        else
                        {
                            if (enableDebugLogs) Debug.LogWarning($"[WorkerManager] Produto nulo na área {task.originID} para Worker {data.id}");
                        }
                    }
                    else
                    {
                        if (enableDebugLogs) Debug.LogError($"[WorkerManager] Área de produção {task.originID} não encontrada para Worker {data.id}!");
                    }
                }
                // Adicionar outros tipos de tarefa conforme necessário
            }
            else
            {
                if (enableDebugLogs) Debug.LogError($"[WorkerManager] Tarefa {data.currentTaskID} não encontrada para Worker {data.id}!");
            }
        }

        private void ExecuteDeliverAction(ref WorkerData data)
        {
            if (data.isCarryingItem && activeTasks.TryGetValue(data.currentTaskID, out var task))
            {
                if (storageStations.TryGetValue(task.destinationID, out var storage))
                {
                    var product = ProductDatabase.GetRawProductByID(data.carriedItemID);
                    if (product != null && storage.ForceAddProduct(product, data.inventoryCount))
                    {
                        if (enableDebugLogs) Debug.Log($"[WorkerManager] Worker {data.id} entregou {product.productName} no armazém {task.destinationID}");
                        data.carriedItemID = 0;
                        data.isCarryingItem = false;
                        data.inventoryCount = 0;
                    }
                    else
                    {
                        if (enableDebugLogs) Debug.LogWarning($"[WorkerManager] Worker {data.id} falhou ao entregar item no armazém {task.destinationID}");
                        FailTask(data.currentTaskID);
                    }
                }
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

        private void TryStoreHarvestedItem(BaseProduct product, int preferredStorageID, int quantity = 1)
        {
            if (enableDebugLogs) Debug.Log($"[WorkerManager] Tentando armazenar {quantity}x {product.productName} (ID: {product.productID})");

            // Tentar primeiro o armazém preferido
            if (preferredStorageID != -1 && storageStations.TryGetValue(preferredStorageID, out var preferredStorage))
            {
                if (preferredStorage.ForceAddProduct(product, quantity))
                {
                    if (enableDebugLogs) Debug.Log($"[WorkerManager] ✓ Armazenado no armazém preferido {preferredStorageID}");
                    return;
                }
            }

            // Procurar armazém compatível
            foreach (var storagePair in storageStations)
            {
                var storage = storagePair.Value;
                if (storage.CanStoreProduct(product) && storage.ForceAddProduct(product, quantity))
                {
                    if (enableDebugLogs) Debug.Log($"[WorkerManager] ✓ Armazenado no armazém compatível {storagePair.Key}");
                    return;
                }
            }

            // Fallback: qualquer armazém com espaço
            foreach (var storagePair in storageStations)
            {
                var storage = storagePair.Value;
                if (storage.inventory.CanStorage() && storage.ForceAddProduct(product, quantity))
                {
                    if (enableDebugLogs) Debug.LogWarning($"[WorkerManager] ⚠ Armazenado no fallback {storagePair.Key}");
                    return;
                }
            }

            Debug.LogError($"[WorkerManager] ❌ CRÍTICO: Nenhum armazém disponível para {product.productName}!");
        }



        private int FindAppropriateStorage(BaseProduct product)
        {
            if (enableDebugLogs) Debug.Log($"[WorkerManager] Procurando armazém para {product.productName}");

            // Primeiro: procurar armazém compatível
            foreach (var pair in storageStations)
            {
                var storage = pair.Value;
                if (storage.CanStoreProduct(product))
                {
                    if (enableDebugLogs) Debug.Log($"[WorkerManager] ✓ Encontrado armazém compatível {pair.Key}");
                    return pair.Key;
                }
            }

            // Fallback: qualquer armazém com espaço
            foreach (var pair in storageStations)
            {
                if (pair.Value.inventory.CanStorage())
                {
                    if (enableDebugLogs) Debug.LogWarning($"[WorkerManager] ⚠ Usando fallback {pair.Key}");
                    return pair.Key;
                }
            }

            Debug.LogError($"[WorkerManager] ❌ Nenhum armazém disponível!");
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
        public Dictionary<int, WorkerTask> GetActiveTasks() => activeTasks;
        public WorkerData? GetWorkerByID(int workerID)
        {
            int index = workerDataList.FindIndex(w => w.id == workerID);
            return index != -1 ? workerDataList[index] : null;
        }
        #endregion
    }
}