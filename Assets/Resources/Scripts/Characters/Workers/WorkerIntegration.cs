// ===== WorkerIntegration.cs (Novo - Para Integra��o com Sistemas Existentes) =====
using UnityEngine;
using Tcp4.Assets.Resources.Scripts.Managers;

namespace Tcp4
{
    /// <summary>
    /// Classe respons�vel por integrar o sistema de trabalhadores com os sistemas existentes
    /// </summary>
    public class WorkerIntegration : MonoBehaviour
    {
        [Header("Integration Settings")]
        [SerializeField] private bool autoCreateHarvestTasks = true;
        [SerializeField] private bool autoCreateRefineTasks = true;
        [SerializeField] private bool autoCreateDrinkTasks = true;

        private void Start()
        {
            SubscribeToSystemEvents();
        }

        private void SubscribeToSystemEvents()
        {
            // Integra��o com ProductionArea
            // Quando uma produ��o termina, criar tarefa de colheita
            SubscribeToProductionEvents();

            // Integra��o com RefinementManager
            // Quando h� produtos brutos dispon�veis, criar tarefas de refinamento
            SubscribeToRefinementEvents();

            // Integra��o com OrderManager (se existir)
            // Quando h� novos pedidos, criar tarefas para baristas
            SubscribeToOrderEvents();
        }

        private void SubscribeToProductionEvents()
        {
            // Encontrar todas as ProductionAreas na cena
            var productionAreas = FindObjectsByType<ProductionArea>(FindObjectsSortMode.None);

            foreach (var area in productionAreas)
            {
                // Assumindo que existe um evento quando a produ��o termina
                area.OnProductionComplete += OnProductionComplete;
            }
        }

        private void SubscribeToRefinementEvents()
        {
            // Se o RefinementManager tiver eventos para quando produtos brutos est�o dispon�veis
            // RefinementManager.Instance.OnRawProductAvailable += OnRawProductAvailable;
        }

        private void SubscribeToOrderEvents()
        {
            // Se houver um sistema de pedidos
            // OrderManager.Instance.OnNewOrder += OnNewDrinkOrder;
        }

        // M�todos de callback para integra��o
        private void OnProductionComplete(ProductionArea area, BaseProduct product)
        {
            if (autoCreateHarvestTasks && WorkerManager.Instance != null)
            {
                // Apenas cria a tarefa. A reserva ser� feita pelo WorkerManager.
                WorkerManager.Instance.CreateHarvestTask(area.areaID, product);

                // REMOVA ESTAS DUAS LINHAS:
                // area.ReserveForWorker();
                // Debug.Log($"[WorkerIntegration] �rea {area.areaID} reservada para trabalhador.");
            }
        }

        private void OnRawProductAvailable(BaseProduct rawProduct)
        {
            if (autoCreateRefineTasks && WorkerManager.Instance != null)
            {
                // Encontrar produto refinado correspondente
                var refinedProduct = RefinementManager.Instance?.Refine(rawProduct);
                if (refinedProduct != null)
                {
                    WorkerManager.Instance.CreateRefineTask(rawProduct, refinedProduct);
                }
            }
        }

        private void OnNewDrinkOrder(Drink drink, int orderID)
        {
            if (autoCreateDrinkTasks && WorkerManager.Instance != null)
            {
                WorkerManager.Instance.CreateDrinkTask(drink, orderID);
            }
        }

        // M�todos p�blicos para criar tarefas manualmente
        public void ManuallyCreateHarvestTask(ProductionArea area)
        {
            // Exemplo de como criar uma tarefa manualmente
            if (area != null && WorkerManager.Instance != null)
            {
                // Assumindo que a �rea tem um produto atual
                // var product = area.GetCurrentProduct();
                // WorkerManager.Instance.CreateHarvestTask(area.areaID, product);
            }
        }

        public void ManuallyCreateRefineTask(BaseProduct rawProduct, BaseProduct refinedProduct)
        {
            WorkerManager.Instance?.CreateRefineTask(rawProduct, refinedProduct);
        }

        public void ManuallyCreateDrinkTask(Drink drink)
        {
            // Gerar ID de pedido �nico
            int orderID = System.DateTime.Now.GetHashCode();
            WorkerManager.Instance?.CreateDrinkTask(drink, orderID);
        }
    }
}
