using System;
using System.Collections.Generic;
using Tcp4.Assets.Resources.Scripts.Systems.Clients;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.AI;
using TMPro;
namespace Tcp4.Assets.Resources.Scripts.Managers
{
    public class ClientManager : Singleton<ClientManager>
    {
        public event Action OnSpawnClient;
        public event Action<Client> OnClientSetup;
        public bool isOpenShop = false;
        [Header("Logica de Spawn (Sua Logica)")]
        [SerializeField] private GameObject clientPrefab;
        [SerializeField] private float baseTime = 20f;
        [SerializeField] private bool canSpawn;
        [SerializeField] private float counter;
        [SerializeField] private float maxCounter;

        [SerializeField] private AnimationCurve spawnRateCurve = new AnimationCurve(
            new Keyframe(9f, 1f), new Keyframe(12f, 2f), new Keyframe(18f, 1f));

        [Header("Configuracoes e Referencias de Cena")]
        [SerializeField] private int maxClients = 24;

        [Header("Client Settings")]
        [SerializeField] private float maxQueueWaitTime = 60f; 

        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform shopEntrance;
        [SerializeField] private Transform streetEnd;
        [SerializeField] private Transform counterPoint;
        [SerializeField] private List<Transform> queueSpots;
        [SerializeField] private List<Transform> seatSpots;
        private List<GameObject> clientPool = new();
        private List<Client> clientComponents = new();
        private List<NavMeshAgent> clientAgents = new();
        private NativeArray<ClientData> clientDataArray;
        private NativeArray<ClientAction> clientActionArray;
        private JobHandle aiJobHandle;
        [SerializeField] private bool[] isQueueSpotOccupied;
        [SerializeField] private bool[] isSeatSpotOccupied;
        public override void Awake()
        {
            base.Awake();
            InitializeClientPool();
            InitializeJobSystemArrays();
        }
        void Start()
        {
            RestartCounter();
            isQueueSpotOccupied = new bool[queueSpots.Count];
            isSeatSpotOccupied = new bool[seatSpots.Count];
        }

        private void InitializeClientPool()
        {
            for (int i = 0; i < maxClients; i++)
            {
                GameObject newClientGO = Instantiate(clientPrefab, Vector3.zero, Quaternion.identity, transform);
                newClientGO.name = $"Client_{i}";
                clientPool.Add(newClientGO);
                clientComponents.Add(newClientGO.GetComponent<Client>());
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
        public void Spawn()
        {
            int clientIndex = FindInactiveClientIndex();
            if (clientIndex == -1) return; 
            OnSpawnClient?.Invoke();
            GameObject clientGO = clientPool[clientIndex];
            clientGO.transform.position = spawnPoint.position;
            clientGO.SetActive(true);
            var clientName = GameAssets.Instance.clientNames[UnityEngine.Random.Range(0, GameAssets.Instance.clientNames.Count)];
            var clientSprite = GameAssets.Instance.clientSprites[UnityEngine.Random.Range(0, GameAssets.Instance.clientSprites.Count)];
            var clientModel = GameAssets.Instance.clientModels[UnityEngine.Random.Range(0, GameAssets.Instance.clientModels.Count)];
            var clientID = GameAssets.GenerateID(5);
            Client clientComponent = clientComponents[clientIndex];
            clientComponent.Setup(clientID, clientName, clientSprite, clientModel);
            clientDataArray[clientIndex] = new ClientData
            {
                isActive = true,
                id = clientIndex,
                currentState = ClientState.WalkingOnStreet,
                currentPosition = clientGO.transform.position,
                moveTarget = shopEntrance.position,
                speed = 2f,
                waitTime = 0f
            };
            OnClientSetup?.Invoke(clientComponent);
            RestartCounter();
        }
        private void Update()
        {
            HandleLogicSpawn();

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
        private void LateUpdate()
        {
            aiJobHandle.Complete();
            for (int i = 0; i < maxClients; i++)
            {
                if (!clientDataArray[i].isActive) continue;

                ClientData data = clientDataArray[i];
                data.currentPosition = clientPool[i].transform.position;
                data.isShopOpen = isOpenShop; 
                clientDataArray[i] = data;

                Client clientComponent = clientComponents[i];
                clientComponent.ControlBubble(false);
                clientComponent.UpdateAnimation();
                clientComponent.debugAction = clientActionArray[i].ToString();
                clientComponent.debugState = clientDataArray[i].currentState.ToString();


                switch (clientActionArray[i])
                {
                    case ClientAction.MoveToTarget:
                        Vector3 targetPosition = DetermineTargetPosition(i);
                        Debug.Log($"manager mandando cliente {i} para o alvo: {targetPosition}"); 
                        clientAgents[i].SetDestination(targetPosition);
                        break;
                    case ClientAction.ShowOrderBubble:
                        Sprite drinkSprite = GetDrinkSpriteFromID(data.orderID);
                        clientComponent.ShowWantedProduct(drinkSprite);
                        clientComponent.ControlBubble(true);
                        float maxWaitTime = 30f;
                        clientComponent.UpdateTimerUI(1f - (data.waitTime / maxWaitTime));
                        break;
                    case ClientAction.GiveReward:
                        ShopManager.Instance.AddMoney(35);
                        ShopManager.Instance.AddStars(UnityEngine.Random.Range(70f, 90f));
                        data.currentState = ClientState.LeavingShop;
                        clientDataArray[i] = data;
                        break;
                    case ClientAction.ApplyPenalty:
                        ShopManager.Instance.AddStars(-0.1f);
                        data.currentState = ClientState.LeavingShop;
                        clientDataArray[i] = data;
                        break;
                    case ClientAction.Deactivate:
                        DeactivateClient(i);
                        break;
                }
            }
        }
        public void ServeClient(Drink d)
        {
            for (int i = 0; i < maxClients; i++)
            {
                if (!clientDataArray[i].isActive ||
                    clientDataArray[i].currentState != ClientState.WaitingForOrder)
                    continue;

                ClientData data = clientDataArray[i];

                if (IsCorrectDrink(d, data.orderID))
                {
                    // Liberar o spot da fila imediatamente
                    if (data.queueSpotIndex != -1)
                    {
                        isQueueSpotOccupied[data.queueSpotIndex] = false;
                        data.queueSpotIndex = -1;
                    }

                    // Decisão: 70% chance de sentar, 30% de sair
                    bool willSit = UnityEngine.Random.Range(0f, 1f) <= 0.7f;

                    if (willSit)
                    {
                        int seatIndex = FindNextAvailableSeatSpot();
                        if (seatIndex != -1)
                        {
                            // Reservar o assento
                            isSeatSpotOccupied[seatIndex] = true;
                            data.seatSpotIndex = seatIndex;
                            data.moveTarget = seatSpots[seatIndex].position;
                            data.currentState = ClientState.GoingToSeat;
                        }
                        else
                        {
                            // Sem assentos disponíveis - cliente sai
                            data.currentState = ClientState.LeavingShop;
                            data.moveTarget = streetEnd.position;
                        }
                    }
                    else
                    {
                        // Cliente decide sair após receber pedido
                        data.currentState = ClientState.LeavingShop;
                        data.moveTarget = streetEnd.position;
                    }

                    // Atualizar dados e dar recompensa
                    clientDataArray[i] = data;
                    clientActionArray[i] = ClientAction.GiveReward;

                    // Avançar fila e chamar próximo cliente
                    AdvanceQueue();
                    CallNextClientToCounter();

                    return;
                }
                else
                {
                    // Bebida errada - cliente insatisfeito
                    data.currentState = ClientState.LeavingShop;
                    data.moveTarget = streetEnd.position;

                    // Liberar spot da fila
                    if (data.queueSpotIndex != -1)
                    {
                        isQueueSpotOccupied[data.queueSpotIndex] = false;
                        data.queueSpotIndex = -1;
                    }

                    clientDataArray[i] = data;
                    clientActionArray[i] = ClientAction.ApplyPenalty;

                    // Avançar fila e chamar próximo cliente
                    AdvanceQueue();
                    CallNextClientToCounter();

                    return;
                }
            }
            Debug.Log($"Nenhum cliente no balcão esperando por {d.name}!");
        }

        // Novo método para chamar próximo cliente ao balcão
        private void CallNextClientToCounter()
        {
            int nextClientIndex = FindClientAtQueueSpot(0);
            if (nextClientIndex != -1)
            {
                ClientData data = clientDataArray[nextClientIndex];
                data.currentState = ClientState.AtCounter;
                data.moveTarget = counterPoint.position;
                clientDataArray[nextClientIndex] = data;
            }
        }
        private void DeactivateClient(int index)
        {
            if (index < 0 || index >= maxClients || !clientPool[index].activeSelf) return;
            clientPool[index].SetActive(false);
            clientDataArray[index] = new ClientData { isActive = false };
        }
        void HandleLogicSpawn()
        {
            if (canSpawn) 
            {
                if (counter >= maxCounter)
                {
                    Spawn();
                }
                else
                {
                    counter += Time.deltaTime * TimeManager.Instance.timeMultiplier;
                }
            }
        }
        void RestartCounter() { counter = 0f; }
        public void StartSpawnClients() { canSpawn = true; }
        public void StopSpawnClients() { canSpawn = false; counter = 0f; }
        public void OpenShop() => isOpenShop = true;
        public void CloseShop() => isOpenShop = false;
        private int FindInactiveClientIndex()
        {
            for (int i = 0; i < maxClients; i++) { if (!clientDataArray[i].isActive) return i; }
            return -1;
        }
        private Vector3 DetermineTargetPosition(int clientIndex)
        {
            ClientData data = clientDataArray[clientIndex];
            switch (data.currentState)
            {
                case ClientState.WalkingOnStreet:
                    return data.moveTarget;
                case ClientState.GoingToQueue:
                    if (data.canQueue)
                    {
                        float distanceToQueueSpot = math.distance(data.currentPosition, queueSpots[data.queueSpotIndex].position);
                        Debug.Log($"Distancia até spot: {distanceToQueueSpot}");
                        return queueSpots[data.queueSpotIndex].position;
                    }
                    int queueIndex = FindNextAvailableQueueSpot();
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
                        data.canQueue = false;
                        data.currentState = ClientState.LeavingShop;
                    }
                    break;
                case ClientState.GoingToSeat:
                    if (data.canSeat)
                    {
                        return queueSpots[data.seatSpotIndex].position;
                    }
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
                    else
                    {
                        data.currentState = ClientState.LeavingShop;
                    }
                    break;
                case ClientState.LeavingShop:
                    if (data.seatSpotIndex != -1)
                        isSeatSpotOccupied[data.seatSpotIndex] = false;
                    data.queueSpotIndex = -1;
                    data.seatSpotIndex = -1;
                    clientDataArray[clientIndex] = data;
                    return streetEnd.position;
            }
            return data.currentPosition;
        }
        private int FindNextAvailableQueueSpot()
        {
            for (int i = 0; i < isQueueSpotOccupied.Length; i++)
            {
                if (isQueueSpotOccupied[i] == false)
                {
                    return i;
                }
            }
            Debug.Log("Nenhum espaço disponivel na fila");
            return -1; 
        }
        private int FindNextAvailableSeatSpot()
        {
            for (int i = 0; i < isSeatSpotOccupied.Length; i++)
            {
                if (isSeatSpotOccupied[i] == false)
                {
                    return i;
                }
            }
            Debug.Log("Nenhum espaço disponivel na cadeira");
            return -1; 
        }
        private Sprite GetDrinkSpriteFromID(int id) { return RefinamentManager.Instance.GetDrinkByID(id).productImage; }
        private bool IsCorrectDrink(Drink drink, int orderID) { return drink.productID == orderID; }
        private void OnDestroy()
        {
            if (clientDataArray.IsCreated) clientDataArray.Dispose();
            if (clientActionArray.IsCreated) clientActionArray.Dispose();
        }

        public void AdvanceQueue()
        {
            // Libera o primeiro spot
            if (isQueueSpotOccupied[0])
            {
                isQueueSpotOccupied[0] = false;

                // Move todos os clientes uma posição para frente
                for (int i = 1; i < queueSpots.Count; i++)
                {
                    if (isQueueSpotOccupied[i])
                    {
                        // Atualiza o spot do cliente
                        int clientIndex = FindClientAtQueueSpot(i);
                        if (clientIndex != -1)
                        {
                            ClientData data = clientDataArray[clientIndex];
                            data.queueSpotIndex = i - 1;
                            data.moveTarget = queueSpots[i - 1].position;
                            clientDataArray[clientIndex] = data;
                            clientAgents[clientIndex].SetDestination(queueSpots[i - 1].position);

                            // Atualiza ocupação
                            isQueueSpotOccupied[i - 1] = true;
                            isQueueSpotOccupied[i] = false;
                        }
                    }
                }
            }
        }

        // Novo método para encontrar cliente em um spot
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

        public void ClientLeftCounter()
        {
            AdvanceQueue();

            // Ativa o próximo cliente
            int nextClientIndex = FindClientAtQueueSpot(0);
            if (nextClientIndex != -1)
            {
                ClientData data = clientDataArray[nextClientIndex];
                data.currentState = ClientState.AtCounter;
                clientDataArray[nextClientIndex] = data;
            }
        }

        private void AdvanceClientToCounter(int queueIndex)
        {
            if (queueIndex != 0) return;

            int clientIndex = FindClientAtQueueSpot(queueIndex);
            if (clientIndex != -1)
            {
                ClientData data = clientDataArray[clientIndex];
                data.currentState = ClientState.AtCounter;
                data.moveTarget = counterPoint.position;
                clientDataArray[clientIndex] = data;
                clientAgents[clientIndex].SetDestination(counterPoint.position);
            }
        }
    }
}