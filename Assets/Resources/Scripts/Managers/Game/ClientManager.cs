using ComponentUtils.ComponentUtils.Scripts;
using System;
using System.Collections.Generic;
using Tcp4.Assets.Resources.Scripts.Systems.Clients;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Tcp4.Assets.Resources.Scripts.Managers
{
    public class ClientManager : Singleton<ClientManager>
    {
        public event Action OnSpawnClient;
        public event Action<Client> OnClientSetup;
        [SerializeField] private List<Transform> clientSpots;
        [SerializeField] private GameObject prefab;
        [SerializeField] private List<GameObject> clients = new();

        [SerializeField] private float baseTime;
        [SerializeField] private bool canSpawn;
        [SerializeField] private float counter;
        [SerializeField] private float maxCounter;
        [SerializeField] private int maxClients = 24;

        public void Start()
        {
            RestartCounter();
        }
        public void Spawn()
        {
            if (clients.Count >= maxClients) return;

            OnSpawnClient?.Invoke();

            var totalStars = ShopManager.Instance.GetStars() / ShopManager.Instance.GetMaxStars() * 5f;

            float stars = UnityEngine.Random.Range(0f, totalStars);
            float minimum = UnityEngine.Random.Range(0.1f, 0.5f + totalStars * 10);

            GameObject _prefab = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            Client prefabClient = _prefab.GetComponent<Client>();
            prefabClient.Setup(stars, minimum);

            OnClientSetup?.Invoke(prefabClient);

            clients.Add(_prefab);

            OrganizeClients();
            RestartCounter();
        }

        public void Update()
        {
            HandleLogicSpawn();
        }

        void RestartCounter()
        { 
            maxCounter = baseTime - ShopManager.Instance.GetStars() / ShopManager.Instance.GetMaxStars() * 2; 
            counter = 0;
        }

        void HandleLogicSpawn()
        {
            if (!canSpawn) return;

            counter += Time.deltaTime;
            if (counter >= maxCounter) Spawn();
        }

        void OrganizeClients()
        {
            for(var i = 0; i < clients.Count; i++)
            {
                clients[i].transform.position = clientSpots[i].position;
            }
            
        }

        void DeleteClients()
        {
            foreach (GameObject c in clients)
            {
                Destroy(c);
            }

            clients.Clear();
        }

        public void DeleteSpecificClient(Client client)
        {

            foreach (GameObject c in clients)
            {
                Client cClient = c.GetComponent<Client>();

                if(cClient.ID == client.ID)
                {
                    clients.Remove(c);
                    break;
                }
            }

            OrganizeClients();
        }

        public void ServeClient(Drink d)
        {
            List<GameObject> listaClientes = clients;

            if(clients == null) 
            {
                listaClientes = clients;
                //garantindo que vou tentar pegar a lista dnv se for nula
            }

            foreach (GameObject client in listaClientes)
            {
                Client c = client.GetComponent<Client>();

                if(c == null)
                {
                    Debug.Log($"Cliente nulo!!");
                    return;
                }
                
                if(c.wantedProduct.name == d.name)
                {
                    Debug.Log($"{d} com {d.quality} de qualidade, entregue para {c}!");
                    c.Delivered();
                    return;
                }
            }

            Debug.Log($"Nenhum cliente precisa de {d}!");
        }

        public void StartSpawnClients() { canSpawn = true; Debug.Log("Posso spawnar clientes!"); }
        public void StopSpawnClients() { canSpawn = false; Debug.Log("NÃO posso spawnar clientes!"); counter = 0f; DeleteClients(); }
}
}