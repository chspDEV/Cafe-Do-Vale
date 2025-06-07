//sem espaco no inicio, sem acentuacao, tudo minusculo
using System;
using System.Collections.Generic;
using Tcp4.Assets.Resources.Scripts.Systems.Clients; //sua namespace para o client.cs
using UnityEngine;

//usings para o job system e navmesh
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.AI;

//[NOTA] certifique-se de que os outros scripts como singleton, timemanager, etc., estao corretos e acessiveis.
namespace Tcp4.Assets.Resources.Scripts.Managers
{
    public class ClientManager : Singleton<ClientManager>
    {
        public event Action OnSpawnClient;
        public event Action<Client> OnClientSetup;

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
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform shopEntrance;
        [SerializeField] private Transform counterPoint;
        [SerializeField] private List<Transform> queueSpots;
        [SerializeField] private List<Transform> seatSpots;

        //estruturas para o job system e pooling
        private List<GameObject> clientPool = new();
        private List<Client> clientComponents = new();
        private List<NavMeshAgent> clientAgents = new();
        private NativeArray<ClientData> clientDataArray;
        private NativeArray<ClientAction> clientActionArray;
        private JobHandle aiJobHandle;
        
        public override void Awake()
        {
            base.Awake();
            InitializeClientPool();
            InitializeJobSystemArrays();
        }

        public void Start()
        {
            RestartCounter();
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

            //ativa o cliente do pool
            GameObject clientGO = clientPool[clientIndex];
            clientGO.transform.position = spawnPoint.position;
            clientGO.SetActive(true);
            
            //pega os dados visuais aleatorios (como era feito no seu client.cs)
            var clientName = GameAssets.Instance.clientNames[UnityEngine.Random.Range(0, GameAssets.Instance.clientNames.Count)];
            var clientSprite = GameAssets.Instance.clientSprites[UnityEngine.Random.Range(0, GameAssets.Instance.clientSprites.Count)];
            var clientModel = GameAssets.Instance.clientModels[UnityEngine.Random.Range(0, GameAssets.Instance.clientModels.Count)];
            var clientID = GameAssets.GenerateID(5);
            
            //chama o setup visual do componente client
            Client clientComponent = clientComponents[clientIndex];
            clientComponent.Setup(clientID, clientName, clientSprite, clientModel);
            
            //configura os dados para o job system
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
        
        public void Update()
        {
            HandleLogicSpawn();

            var job = new ClientDecisionJob
            {
                clientDataArray = this.clientDataArray,
                actionArray = this.clientActionArray,
                deltaTime = Time.deltaTime,
                playerReputation = ShopManager.Instance.GetStars() / ShopManager.Instance.GetMaxStars(),
                shopEntrancePosition = shopEntrance.position
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
                clientDataArray[i] = data;

                Client clientComponent = clientComponents[i];

                //atualiza a ui do timer continuamente
                float maxWaitTime = 30f; //[NOTA] defina o tempo maximo de espera aqui
                clientComponent.UpdateTimerUI(1f - (data.waitTime / maxWaitTime));

                switch (clientActionArray[i])
                {
                    case ClientAction.MoveToTarget:
                        clientAgents[i].SetDestination(DetermineTargetPosition(data.currentState));
                        break;
                        
                    case ClientAction.ShowOrderBubble:
                        Sprite drinkSprite = GetDrinkSpriteFromID(data.orderID);
                        clientComponent.ShowWantedProduct(drinkSprite);
                        break;
                    
                    case ClientAction.GiveReward:
                        ShopManager.Instance.AddMoney(35);
                        ShopManager.Instance.AddStars(UnityEngine.Random.Range(70f, 90f));
                        //muda o estado para o proximo passo (ex: ir sentar ou sair)
                        //[NOTA] esta logica deve ser mais robusta
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
        
        //[NOTA] esta funcao deve ser adaptada para sua logica de jogo
        public void ServeClient(Drink d)
        {
            for (int i = 0; i < maxClients; i++)
            {
                if (!clientDataArray[i].isActive || clientDataArray[i].currentState != ClientState.WaitingForOrder) continue;

                ClientData data = clientDataArray[i];
                //[NOTA] aqui voce precisa de uma forma de comparar a 'drink d' com o 'data.orderid'
                if (IsCorrectDrink(d, data.orderID))
                {
                    //pedido correto!
                    data.currentState = ClientState.GoingToSeat; //ou leaving shop
                    data.moveTarget = FindFreeSeatPosition();
                    clientDataArray[i] = data;
                    //forca a acao de recompensa no proximo frame
                    clientActionArray[i] = ClientAction.GiveReward; 
                    return; 
                }
            }
            Debug.Log($"nenhum cliente esperando por {d.name}!");
        }

        private void DeactivateClient(int index)
        {
            if (index < 0 || index >= maxClients || !clientPool[index].activeSelf) return;
            clientPool[index].SetActive(false);
            clientDataArray[index] = new ClientData { isActive = false };
        }

        //funcoes de controle de spawn mantidas
        void HandleLogicSpawn() { if (canSpawn) { counter += Time.deltaTime; if (counter >= maxCounter) Spawn(); } }
        void RestartCounter() { /* ... sua logica original ... */ }
        public void StartSpawnClients() { canSpawn = true; }
        public void StopSpawnClients() { canSpawn = false; counter = 0f; /*... logica de deletar todos ...*/ }

        //funcoes auxiliares
        private int FindInactiveClientIndex()
        {
            for (int i = 0; i < maxClients; i++) { if (!clientDataArray[i].isActive) return i; }
            return -1;
        }

        private Vector3 DetermineTargetPosition(ClientState state)
        {
            //[NOTA] funcao de exemplo. implemente sua logica para encontrar o proximo ponto na fila, etc.
            if (state == ClientState.GoingToQueue && queueSpots.Count > 0) return queueSpots[0].position;
            if (state == ClientState.LeavingShop) return spawnPoint.position; //volta para o spawn para sair
            return transform.position; //fallback
        }

        private Sprite GetDrinkSpriteFromID(int id) { /*[NOTA] implemente a logica para pegar o sprite do produto pelo id*/ return null; }
        private bool IsCorrectDrink(Drink drink, int orderID) { /*[NOTA] implemente a logica de comparacao*/ return false; }
        private Vector3 FindFreeSeatPosition() { /*[NOTA] implemente a logica para encontrar uma cadeira livre*/ return seatSpots[0].position; }
        
        private void OnDestroy()
        {
            if (clientDataArray.IsCreated) clientDataArray.Dispose();
            if (clientActionArray.IsCreated) clientActionArray.Dispose();
        }
    }
}