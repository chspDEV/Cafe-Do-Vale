using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Tcp4.Assets.Resources.Scripts.Managers;

namespace Tcp4
{
    public class WorkerUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button hireFarmerButton;
        [SerializeField] private Button hireRepositorButton;
        [SerializeField] private Button hireBaristaButton;
        [SerializeField] private TextMeshProUGUI workerCountText;
        [SerializeField] private Transform workerListParent;
        [SerializeField] private GameObject workerInfoPrefab;

        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI pendingTasksText;
        [SerializeField] private TextMeshProUGUI activeTasksText;
        [SerializeField] private TextMeshProUGUI completedTasksText;

        [Header("Hiring Costs")] // NOVO
        [SerializeField] private TextMeshProUGUI farmerCostText;
        [SerializeField] private TextMeshProUGUI repositorCostText;
        [SerializeField] private TextMeshProUGUI baristaCostText;
        [SerializeField] private TextMeshProUGUI farmerDailyText;
        [SerializeField] private TextMeshProUGUI repositorDailyText;
        [SerializeField] private TextMeshProUGUI baristaDailyText;
        [SerializeField] private TextMeshProUGUI moneyText; // NOVO

        private void Start()
        {
            SetupButtons();
            SubscribeToEvents();
            UpdateFullUI(); // Atualiza toda a UI ao iniciar
        }

        private void SetupButtons()
        {
            hireFarmerButton?.onClick.AddListener(() => HireWorker(WorkerType.Fazendeiro));
            hireRepositorButton?.onClick.AddListener(() => HireWorker(WorkerType.Repositor));
            hireBaristaButton?.onClick.AddListener(() => HireWorker(WorkerType.Barista));
        }

        private void SubscribeToEvents()
        {
            if (WorkerManager.Instance != null)
            {
                WorkerManager.Instance.OnWorkerHired += OnWorkerManagerHired;
                WorkerManager.Instance.OnWorkerFired += OnWorkerFired;
                WorkerManager.Instance.OnTaskCompleted += OnTaskCompleted;
            }

            // NOVO: Eventos econômicos
            if (WorkerEconomics.Instance != null)
            {
                WorkerEconomics.Instance.OnWorkerHired += OnWorkerEconomicsHired;
            }

            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnChangeMoney += OnMoneyChanged;
            }
        }

        private void Update()
        {
            UpdateWorkerStats();
        }

        // NOVO: Método separado para atualizar estatísticas
        private void UpdateWorkerStats()
        {
            if (WorkerManager.Instance == null) return;

            var workers = WorkerManager.Instance.GetActiveWorkers();
            var farmers = workers.Count(w => w.type == WorkerType.Fazendeiro);
            var repositors = workers.Count(w => w.type == WorkerType.Repositor);
            var baristas = workers.Count(w => w.type == WorkerType.Barista);

            workerCountText.text = $"Fazendeiros: {farmers} | Repositores: {repositors} | Baristas: {baristas}";

            var pendingTasks = WorkerManager.Instance.GetPendingTasks().Count;
            var activeTasks = WorkerManager.Instance.GetActiveTasks().Count;

            pendingTasksText.text = $"Pendentes: {pendingTasks}";
            activeTasksText.text = $"Ativas: {activeTasks}";
        }

        // NOVO: Atualiza toda a UI
        private void UpdateFullUI()
        {
            UpdateWorkerStats();
            UpdateCostDisplays();
            UpdateMoneyDisplay();
            UpdateButtonsInteractable();
        }

        // NOVO: Atualiza displays de custo
        private void UpdateCostDisplays()
        {
            if (WorkerEconomics.Instance == null) return;

            farmerCostText.text = $"Contratação: ${WorkerEconomics.Instance.GetHiringCost(WorkerType.Fazendeiro)}";
            farmerDailyText.text = $"Diário: ${WorkerEconomics.Instance.GetDailyCost(WorkerType.Fazendeiro)}";

            repositorCostText.text = $"Contratação: ${WorkerEconomics.Instance.GetHiringCost(WorkerType.Repositor)}";
            repositorDailyText.text = $"Diário: ${WorkerEconomics.Instance.GetDailyCost(WorkerType.Repositor)}";

            baristaCostText.text = $"Contratação: ${WorkerEconomics.Instance.GetHiringCost(WorkerType.Barista)}";
            baristaDailyText.text = $"Diário: ${WorkerEconomics.Instance.GetDailyCost(WorkerType.Barista)}";
        }

        // NOVO: Atualiza display de dinheiro
        private void UpdateMoneyDisplay()
        {
            if (ShopManager.Instance != null)
            {
                moneyText.text = $"Dinheiro: ${ShopManager.Instance.GetMoney()}";
            }
        }

        // NOVO: Atualiza estado dos botões
        private void UpdateButtonsInteractable()
        {
            if (WorkerEconomics.Instance == null) return;

            hireFarmerButton.interactable = WorkerEconomics.Instance.CanAffordWorker(WorkerType.Fazendeiro);
            hireRepositorButton.interactable = WorkerEconomics.Instance.CanAffordWorker(WorkerType.Repositor);
            hireBaristaButton.interactable = WorkerEconomics.Instance.CanAffordWorker(WorkerType.Barista);
        }

        // NOVO: Reage a mudanças no dinheiro
        private void OnMoneyChanged()
        {
            UpdateMoneyDisplay();
            UpdateButtonsInteractable();
        }

        private void HireWorker(WorkerType type)
        {
            if (WorkerEconomics.Instance != null)
            {
                WorkerEconomics.Instance.TryHireWorker(type);
            }
            else
            {
                Debug.LogError("WorkerEconomics não encontrado!");
            }
        }

        // RENOMEADO: Para evitar conflito
        private void OnWorkerManagerHired(WorkerData worker)
        {
            Debug.Log($"Interface: Novo trabalhador contratado - {worker.type}");
            UpdateWorkerStats(); // Atualiza contagem
        }

        // NOVO: Handler para evento de WorkerEconomics
        private void OnWorkerEconomicsHired(WorkerType type, int cost)
        {
            Debug.Log($"Trabalhador {type} contratado por ${cost}");
            UpdateFullUI(); // Atualiza toda a UI
        }

        private void OnWorkerFired(int workerID)
        {
            Debug.Log($"Interface: Trabalhador {workerID} foi demitido");
            UpdateWorkerStats(); // Atualiza contagem
        }

        private void OnTaskCompleted(WorkerTask task)
        {
            Debug.Log($"Interface: Tarefa {task.taskID} completada");
        }

        private void OnDestroy()
        {
            if (WorkerManager.Instance != null)
            {
                WorkerManager.Instance.OnWorkerHired -= OnWorkerManagerHired;
                WorkerManager.Instance.OnWorkerFired -= OnWorkerFired;
                WorkerManager.Instance.OnTaskCompleted -= OnTaskCompleted;
            }

            // NOVO: Limpar registros econômicos
            if (WorkerEconomics.Instance != null)
            {
                WorkerEconomics.Instance.OnWorkerHired -= OnWorkerEconomicsHired;
            }

            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnChangeMoney -= OnMoneyChanged;
            }
        }
    }
}