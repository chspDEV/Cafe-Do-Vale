using System;
using System.Collections.Generic;
using Tcp4.Assets.Resources.Scripts.Systems.Clients;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.AI;
using TMPro;
using Sirenix.OdinInspector; 

namespace Tcp4.Assets.Resources.Scripts.Managers
{
    public class ClientManager : Singleton<ClientManager>
    {
        #region Events
        public event Action OnSpawnClient;
        public event Action<Client> OnClientSetup;
        #endregion

        #region Serialized Fields
        [Header("Spawn Settings")]
        [SerializeField] private GameObject clientPrefab;
        [SerializeField] private float clientWaitOrderTime = 60f;
        [SerializeField] private float clientWaitQueueTime = 120f;
        [SerializeField] private float maxCounter;
        [SerializeField]
        private AnimationCurve spawnRateCurve = new AnimationCurve(
            new Keyframe(9f, 1f), new Keyframe(12f, 2f), new Keyframe(18f, 1f));

        [Header("Scene References")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform shopEntrance;
        [SerializeField] private Transform streetEnd;
        [SerializeField] private Transform counterPoint;
        [SerializeField] private List<Transform> queueSpots;
        [SerializeField] private List<Transform> seatSpots;

        [Header("Client Settings")]
        [SerializeField] private int maxClients = 24;
        [SerializeField] private float maxQueueWaitTime = 60f;
        [SerializeField] private bool[] isQueueSpotOccupied;
        [SerializeField] private bool[] isSeatSpotOccupied;
        #endregion

        #region Private Variables
        private List<GameObject> clientPool = new();
        private List<Client> clientComponents = new();
        private List<NavMeshAgent> clientAgents = new();
        private NativeArray<ClientData> clientDataArray;
        private NativeArray<ClientAction> clientActionArray;
        private JobHandle aiJobHandle;
        private float counter;
        #endregion

        #region Public Properties
        [ShowInInspector, Sirenix.OdinInspector.ReadOnly] public bool isOpenShop { get; private set; }
        [ShowInInspector, Sirenix.OdinInspector.ReadOnly] public bool canSpawn { get; private set; }
        [ShowInInspector, Sirenix.OdinInspector.ReadOnly] public float currentCounter { get; private set; }
        #endregion

        #region Unity Methods
        public override void Awake()
        {
            base.Awake();
            InitializeClientPool();
            InitializeJobSystemArrays();
        }

        void Start()
        {
            RestartCounter();
            ClearQueueAndSeats();           
        }

        private void Update()
        {
            HandleLogicSpawn();
            UpdateClientTimers();

            if (aiJobHandle.IsCompleted)
            {
                ScheduleAIJob();
            }
        }

        private void LateUpdate()
        {
            aiJobHandle.Complete();
            ProcessClientActions();
        }

        private void OnDestroy()
        {
            CleanupJobSystemArrays();
        }
        #endregion

        #region Initialization
        private void InitializeClientPool()
        {
            for (int i = 0; i < maxClients; i++)
            {
                GameObject newClientGO = Instantiate(clientPrefab, Vector3.zero, Quaternion.identity, transform);
                newClientGO.name = $"Client_{i}";
                clientPool.Add(newClientGO);

                var clientComponent = newClientGO.GetComponent<Client>();
                clientComponents.Add(clientComponent);

                if (!newClientGO.TryGetComponent<NavMeshAgent>(out var agent))
                {
                    agent = newClientGO.AddComponent<NavMeshAgent>();
                }
                clientAgents.Add(agent);

                newClientGO.SetActive(false);
            }
        }

        private void InitializeJobSystemArrays()
        {
            clientDataArray = new NativeArray<ClientData>(maxClients, Allocator.Persistent);
            clientActionArray = new NativeArray<ClientAction>(maxClients, Allocator.Persistent);

            for (int i = 0; i < maxClients; i++)
            {
                clientDataArray[i] = new ClientData { isActive = false };
            }
        }

        private void CleanupJobSystemArrays()
        {
            if (clientDataArray.IsCreated) clientDataArray.Dispose();
            if (clientActionArray.IsCreated) clientActionArray.Dispose();
        }
        #endregion

        #region Client Spawn Management
        public void Spawn()
        {
            int clientIndex = FindInactiveClientIndex();
            if (clientIndex == -1) return;

            OnSpawnClient?.Invoke();
            GameObject clientGO = clientPool[clientIndex];
            clientGO.transform.position = spawnPoint.position;
            clientGO.SetActive(true);

            var clientSprite = GameAssets.Instance.clientSprites[UnityEngine.Random.Range(0, GameAssets.Instance.clientSprites.Count)];
            var clientModel = GameAssets.Instance.clientModels[UnityEngine.Random.Range(0, GameAssets.Instance.clientModels.Count)];
            var clientID = GameAssets.GenerateID(5);

            Client clientComponent = clientComponents[clientIndex];
            clientComponent.Setup(clientID, clientSprite, clientModel);

            clientDataArray[clientIndex] = new ClientData
            {
                isActive = true,
                id = clientIndex,
                currentState = ClientState.WalkingOnStreet,
                currentPosition = clientGO.transform.position,
                moveTarget = shopEntrance.position,
                speed = 2f,
                waitQueueTime = 0f,
                waitOrderTime = 0f
            };

            OnClientSetup?.Invoke(clientComponent);
            RestartCounter();
        }

        private void HandleLogicSpawn()
        {
            currentCounter = counter;

            float currentHour = TimeManager.Instance.CurrentHour;

            float spawnMultiplier = spawnRateCurve.Evaluate(currentHour);

            float adjustedSpawnInterval = maxCounter / Mathf.Max(spawnMultiplier, 0.1f);

            canSpawn = currentHour < 22f && currentHour > 5f;

            if (!canSpawn) return;

            if (counter >= adjustedSpawnInterval)
            {
                Spawn();
            }
            else
            {
                counter += Time.deltaTime * TimeManager.Instance.timeMultiplier;
            }
        }

        [Button("Start Spawning")]
        public void StartSpawnClients() { canSpawn = true; RestartCounter(); }

        [Button("Stop Spawning")]
        public void StopSpawnClients() { canSpawn = false; RestartCounter(); }

        private void RestartCounter() { counter = 0f; }
        #endregion

        #region Shop Management
        [Button("Open Shop")]
        public void OpenShop() => isOpenShop = true;

        [Button("Close Shop")]
        public void CloseShop() => isOpenShop = false;
        #endregion

        #region AI Job System
        private void ScheduleAIJob()
        {
            var job = new ClientDecisionJob
            {
                clientDataArray = this.clientDataArray,
                actionArray = this.clientActionArray,
                deltaTime = Time.deltaTime,
                playerReputation = ShopManager.Instance.GetStars() / ShopManager.Instance.GetMaxStars(),
                shopEntrancePosition = shopEntrance.position,
                streetEndPosition = streetEnd.position,
                counterPosition = counterPoint.position,
                isShopOpen = isOpenShop
            };
            aiJobHandle = job.Schedule(maxClients, 32);
        }

        private void ProcessClientActions()
        {
            aiJobHandle.Complete();

            for (int i = 0; i < maxClients; i++)
            {
                if (!clientDataArray[i].isActive) continue;

                UpdateClientData(i);
                CheckUnsatisfied(i);
                ProcessClientAction(i);
            }
        }

        private void UpdateClientTimers()
        {
            for (int i = 0; i < maxClients; i++)
            {
                if (!clientDataArray[i].isActive) continue;

                ClientData data = clientDataArray[i];
                Client client = clientComponents[i];

                if (data.currentState == ClientState.WaitingForOrder)
                {
                    float fillAmount = 1f - (data.waitOrderTime / data.maxWaitOrderTime);
                    client.UpdateTimerAtCount(fillAmount);
                }
                else if (data.currentState == ClientState.InQueue)
                {
                    float fillAmount = 1f - (data.waitQueueTime / data.maxWaitQueueTime);
                    client.UpdateTimerAtQueue(fillAmount);
                }
            }
        }

        private void UpdateClientData(int index)
        {
            ClientData data = clientDataArray[index];
            data.currentPosition = clientPool[index].transform.position;
            data.isShopOpen = isOpenShop;
            data.maxWaitOrderTime = clientWaitOrderTime;
            data.maxWaitQueueTime = clientWaitQueueTime;
            clientDataArray[index] = data;

            Client clientComponent = clientComponents[index];
            clientComponent.UpdateAnimation();
            clientComponent.debugAction = clientActionArray[index].ToString();
            clientComponent.debugState = clientDataArray[index].currentState.ToString();
        }

        private void CheckUnsatisfied(int index)
        {
            if (clientDataArray[index].currentState == ClientState.InQueue &&
                clientDataArray[index].waitQueueTime > clientDataArray[index].maxWaitQueueTime)
            {
                HandleUnsatisfiedClient(index);
            }
        }

        private void ProcessClientAction(int index)
        {
            switch (clientActionArray[index])
            {
                case ClientAction.MoveToTarget:
                    clientAgents[index].SetDestination(DetermineTargetPosition(index));
                    break;

                case ClientAction.ShowOrderBubble:
                    ShowOrderBubble(index);
                    break;

                case ClientAction.ApplyPenalty:
                    HandleUnsatisfiedClient(index);
                    break;

                case ClientAction.WaitOrder:
                    ProcessWaitOrder(index);
                    break;

                case ClientAction.Deactivate:
                    DeactivateClient(index);
                    break;
                case ClientAction.RequestQueueSpot:
                    HandleQueueSpotRequest(index);
                    break;
            }
        }
        #endregion

        #region Client Actions
        private void ShowOrderBubble(int index)
        {
            ClientData data = clientDataArray[index];

            data.orderID = GetAnyDrink().productID;

            Sprite drinkSprite = GetDrinkSpriteFromID(data.orderID);
            String drinkName = GetDrinkNameFromID(data.orderID);

            clientComponents[index].ShowWantedProduct(drinkSprite);
            clientComponents[index].UpdateOrderName(drinkName);

            clientComponents[index].ControlQueueBubble(false);
            clientComponents[index].ControlOrderBubble(true);
            
            data.waitOrderTime = 0f;
            clientDataArray[index] = data;
        }

        private void HideOrderBubble(int index)
        {
            ClientData data = clientDataArray[index];
            clientComponents[index].ControlOrderBubble(false);
            data.waitOrderTime = 0f;
            clientDataArray[index] = data;
        }

        private void GiveClientReward(int index)
        {
            ClientData data = clientDataArray[index];
            Debug.Log($"Cliente {data.id} deu uma recompensa!");
            ShopManager.Instance.AddMoney(35); //MELHORAR ISSO DEPOIS
            ShopManager.Instance.AddStars(UnityEngine.Random.Range(70f, 90f));
            HideOrderBubble(index);
        }

        private void ProcessWaitOrder(int index)
        {
            ClientData data = clientDataArray[index];
            data.waitOrderTime += Time.deltaTime;
            clientDataArray[index] = data;

            if (data.waitOrderTime >= data.maxWaitOrderTime)
            {
                HandleUnsatisfiedClient(index);
            }
        }

        private void HandleUnsatisfiedClient(int index)
        {
            ClientData data = clientDataArray[index];

            Debug.Log("Cliente Insatisfeito!");

            if (data.queueSpotIndex != -1)
            {
                isQueueSpotOccupied[data.queueSpotIndex] = false;
                data.queueSpotIndex = -1;
            }

            if (data.seatSpotIndex != -1)
            {
                isSeatSpotOccupied[data.seatSpotIndex] = false;
                data.seatSpotIndex = -1;
            }

            data.currentState = ClientState.LeavingShop;
            data.moveTarget = streetEnd.position;
            clientDataArray[index] = data;
            clientActionArray[index] = ClientAction.ApplyPenalty;
            ShopManager.Instance.AddStars(-0.1f); //MELHORAR ISSO DEPOIS
            PlaytestManager.Instance.RecordClientMissed();

            if (data.queueSpotIndex == 0)
            {
                AdvanceQueue();
            }
        }

        private void HandleQueueSpotRequest(int index)
        {
            Debug.Log($"Cliente {index} solicitando spot na fila");

            ClientData data = clientDataArray[index];
            int queueIndex = FindAvailableQueueSpot();

            if (queueIndex != -1 && queueIndex < queueSpots.Count)
            {
                Debug.Log($"Spot {queueIndex} atribuído ao cliente {index}");
                isQueueSpotOccupied[queueIndex] = true;
                data.queueSpotIndex = queueIndex;
                data.canQueue = true;
                data.moveTarget = queueSpots[queueIndex].position;
                clientDataArray[index] = data;

                if (data.queueSpotIndex != 0)
                {
                    clientAgents[index].SetDestination(queueSpots[queueIndex].position);
                }
                else
                {
                    clientAgents[index].SetDestination(counterPoint.position);
                    data.currentState = ClientState.GoingToCounter;
                }
            }
            else
            {
                Debug.Log($"Nenhum spot disponível para cliente {index}");
                data.canQueue = false;
                data.currentState = ClientState.LeavingShop;
                data.moveTarget = streetEnd.position;
                clientDataArray[index] = data;
            }
        }

        private void DeactivateClient(int index)
        {
            if (index < 0 || index >= maxClients || !clientPool[index].activeSelf) return;

            clientPool[index].SetActive(false);
            clientPool[index].transform.position = spawnPoint.position;
            clientDataArray[index] = new ClientData { isActive = false };
        }
        #endregion

        #region Queue Management
        public void ServeClient(Drink drink)
        {
            // Completa o job antes de acessar os dados
            aiJobHandle.Complete();

            for (int i = 0; i < maxClients; i++)
            {
                if (!clientDataArray[i].isActive ||
                    clientDataArray[i].currentState != ClientState.WaitingForOrder)
                    continue;

                ClientData data = clientDataArray[i];

                // Esconde o bubble imediatamente
                clientComponents[i].ControlOrderBubble(false);

                if (IsCorrectDrink(drink, data.orderID))
                {
                    HandleCorrectDrinkServed(i, data);
                    return;
                }
                else
                {
                    HandleUnsatisfiedClient(i);
                    return;
                }
            }
            Debug.Log($"Nenhum cliente no balcão esperando por {drink.name}!");
        }

        private void HandleCorrectDrinkServed(int index, ClientData data)
        {
            // Libera o spot da fila
            if (data.queueSpotIndex != -1)
            {
                if (data.queueSpotIndex < isQueueSpotOccupied.Length)
                {
                    isQueueSpotOccupied[data.queueSpotIndex] = false;
                }
                data.queueSpotIndex = -1;
            }

            PlaytestManager.Instance.RecordClientServed();
            GiveClientReward(index);

            // Decisão: 70% chance de sentar, 30% de sair
            bool willSit = UnityEngine.Random.Range(0f, 1f) <= 0.7f;

            AdvanceQueue();

            if (willSit)
            {
                int seatIndex = FindNextAvailableSeatSpot();
                if (seatIndex != -1)
                {
                    isSeatSpotOccupied[seatIndex] = true;
                    data.seatSpotIndex = seatIndex;
                    data.moveTarget = seatSpots[seatIndex].position;
                    data.currentState = ClientState.GoingToSeat;
                }
                else
                {
                    data.currentState = ClientState.LeavingShop;
                    data.moveTarget = streetEnd.position;
                }
            }
            else
            {
                data.currentState = ClientState.LeavingShop;
                data.moveTarget = streetEnd.position;
            }

            clientDataArray[index] = data;
        }


        public void AdvanceQueue()
        {
            // Verifica se o primeiro spot está ocupado
            //if (!isQueueSpotOccupied[0]) return;

            Debug.Log("TENTANDO AVANÇAR FILA!");
            // Libera o primeiro spot
            isQueueSpotOccupied[0] = false;

            // Move todos os clientes uma posição para frente
            for (int i = 1; i < queueSpots.Count; i++)
            {
                Debug.Log("[CHECANDO POSICOES DE CLIENTE]");

                if (isQueueSpotOccupied[i])
                {
                    int clientIndex = FindClientAtQueueSpot(i);
                    if (clientIndex != -1)
                    {
                        ClientData data = clientDataArray[clientIndex];
                        data.queueSpotIndex = i - 1;
                        data.moveTarget = queueSpots[i - 1].position;

                        Debug.Log($"Cliente na posicao {i} foi para {data.queueSpotIndex}");
                        clientDataArray[clientIndex] = data;

                        // Atualiza ocupação
                        isQueueSpotOccupied[i - 1] = true;
                        isQueueSpotOccupied[i] = false;

                        // Força o movimento imediato
                        clientAgents[clientIndex].SetDestination(queueSpots[i - 1].position);

                    }
                }
            }

            // Chama o próximo cliente para o balcão
            CallNextClientToCounter();
        }

        public void ClearQueueAndSeats()
        {
            isQueueSpotOccupied = new bool[Mathf.Max(queueSpots.Count, 1)];
            isSeatSpotOccupied = new bool[Mathf.Max(seatSpots.Count, 1)];
        }

        private void CallNextClientToCounter()
        {
            int nextClientIndex = FindClientAtQueueSpot(0);

            if (nextClientIndex != -1)
            {
                ClientData data = clientDataArray[nextClientIndex];
                data.currentState = ClientState.AtCounter;
                data.moveTarget = counterPoint.position;
                clientDataArray[nextClientIndex] = data;
                Debug.Log($"AVANÇANDO CLIENTE {data.id} PARA O BALCÃO!");
                // Força o movimento imediato
                clientAgents[nextClientIndex].SetDestination(counterPoint.position);
            }
        }

        [Button("Force Advance Queue")]
        public void ClientLeftCounter()
        {
            AdvanceQueue();
            CallNextClientToCounter();
        }
        #endregion

        #region Utility Methods
        private int FindInactiveClientIndex()
        {
            for (int i = 0; i < maxClients; i++)
            {
                if (!clientDataArray[i].isActive) return i;
            }
            return -1;
        }

        private Vector3 DetermineTargetPosition(int clientIndex)
        {
            ClientData data = clientDataArray[clientIndex];

            switch (data.currentState)
            {
                case ClientState.WalkingOnStreet:
                    return data.moveTarget; // Já deve ser shopEntrance.position

                case ClientState.GoingToQueue:
                    if (data.queueSpotIndex >= 0 && data.queueSpotIndex < queueSpots.Count)
                    {

                        clientComponents[clientIndex].ControlQueueBubble(true);
                        return queueSpots[data.queueSpotIndex].position;
                    }
                    else
                    {
                        // Fallback seguro
                        Debug.LogWarning($"Índice inválido de fila: {data.queueSpotIndex} para cliente {clientIndex}");
                        data.currentState = ClientState.LeavingShop;
                        clientDataArray[clientIndex] = data;
                        return streetEnd.position;
                    }



                case ClientState.GoingToSeat:
                    if (data.canSeat)
                    {
                        return seatSpots[data.seatSpotIndex].position;
                    }
                    return HandleSeatSpotAssignment(clientIndex, data);

                        case ClientState.LeavingShop:
                            if (data.seatSpotIndex != -1)
                                isSeatSpotOccupied[data.seatSpotIndex] = false;

                            data.queueSpotIndex = -1;
                            data.seatSpotIndex = -1;
                            clientDataArray[clientIndex] = data;
                            return streetEnd.position;

                        default:
                            return data.currentPosition;
                        }
        }

        private Vector3 HandleQueueSpotAssignment(int clientIndex, ClientData data)
        {

            // Se chegou perto do shopEntrance, tenta conseguir um spot
            float distToEntrance = math.distance(data.currentPosition, shopEntrance.position);

            if (distToEntrance <= 5f)
            {
                int queueIndex = FindAvailableQueueSpot();

                if (queueIndex != -1)
                {
                    isQueueSpotOccupied[queueIndex] = true;
                    data.queueSpotIndex = queueIndex;
                    data.canQueue = true;
                    data.moveTarget = queueSpots[queueIndex].position;
                    clientDataArray[clientIndex] = data;
                    return queueSpots[queueIndex].position;
                }
                else
                {
                    Debug.Log("Nao consegui um spot disponivel");
                    data.canQueue = false;
                    data.currentState = ClientState.LeavingShop;
                    clientDataArray[clientIndex] = data;
                    return streetEnd.position;
                }
            }
            else
            {
                // Continua indo para a entrada da loja
                return shopEntrance.position;
            }
        }

        private Vector3 HandleSeatSpotAssignment(int clientIndex, ClientData data)
        {
            int seatIndex = FindNextAvailableSeatSpot();
            if (seatIndex != -1)
            {
                if (data.queueSpotIndex != -1)
                {
                    isQueueSpotOccupied[data.queueSpotIndex] = false;
                    data.canQueue = false;
                }

                isSeatSpotOccupied[seatIndex] = true;
                data.seatSpotIndex = seatIndex;
                data.canSeat = true;
                data.moveTarget = seatSpots[seatIndex].position;
                clientDataArray[clientIndex] = data;
                return seatSpots[seatIndex].position;
            }

            data.currentState = ClientState.LeavingShop;
            clientDataArray[clientIndex] = data;
            return streetEnd.position;
        }

        private int FindAvailableQueueSpot()
        {
            for (int i = 0; i < isQueueSpotOccupied.Length; i++)
            {
                if (!isQueueSpotOccupied[i]) return i;
            }
            return -1;
        }

        private int FindNextAvailableSeatSpot()
        {
            for (int i = 0; i < isSeatSpotOccupied.Length; i++)
            {
                if (!isSeatSpotOccupied[i]) return i;
            }
            return -1;
        }

        private int FindClientAtQueueSpot(int spotIndex)
        {
            for (int i = 0; i < maxClients; i++)
            {
                if (clientDataArray[i].isActive &&
                    clientDataArray[i].queueSpotIndex == spotIndex)
                {
                    return i;
                }
            }
            return -1;
        }

        private Sprite GetDrinkSpriteFromID(int id)
        {
            return RefinementManager.Instance.GetDrinkByID(id).productImage;
        }

        private String GetDrinkNameFromID(int id)
        {
            return RefinementManager.Instance.GetDrinkByID(id).productName;
        }

        private Drink GetAnyDrink()
        {
            var currentMenu = UnlockManager.Instance.CurrentMenu;
            return currentMenu[UnityEngine.Random.Range(0, currentMenu.Count)];
        }

        private bool IsCorrectDrink(Drink drink, int orderID)
        {
            return drink.productID == orderID;
        }

        private void OnDrawGizmos()
        {
            if (shopEntrance != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(shopEntrance.position, 3f);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(shopEntrance.position, 0.5f);
            }
        }
        #endregion
    }
}