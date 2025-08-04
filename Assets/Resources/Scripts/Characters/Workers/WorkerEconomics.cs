using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Tcp4.Assets.Resources.Scripts.Managers;

namespace Tcp4
{
    [System.Serializable]
    public class WorkerCostData
    {
        [Header("Custos Iniciais")]
        public int hiringCost = 100;           // Custo para contratar

        [Header("Custos Diários")]
        public int dailyWage = 25;             // Salário por dia
        public int dailyMaintenance = 5;       // Custos de manutenção/equipamentos

        [Header("Custos de Performance")]
        public int efficiencyBonus = 10;      // Bônus para trabalhadores eficientes
        public int inefficiencyPenalty = 15;  // Penalidade para trabalhadores ineficientes

        public int GetTotalDailyCost() => dailyWage + dailyMaintenance;
    }

    public class WorkerEconomics : Singleton<WorkerEconomics>
    {
        [Header("Configurações de Custos")]
        [SerializeField]
        private WorkerCostData fazendeiroCosts = new WorkerCostData
        {
            hiringCost = 150,
            dailyWage = 30,
            dailyMaintenance = 8
        };

        [SerializeField]
        private WorkerCostData repositorCosts = new WorkerCostData
        {
            hiringCost = 200,
            dailyWage = 40,
            dailyMaintenance = 12
        };

        [SerializeField]
        private WorkerCostData baristaCosts = new WorkerCostData
        {
            hiringCost = 250,
            dailyWage = 50,
            dailyMaintenance = 15
        };

        [Header("Configurações de Pagamento")]
        [SerializeField] private float paymentHour = 18f;  // Hora do pagamento (fim do expediente)
        [SerializeField] private bool autoPayWorkers = true;
        [SerializeField] private bool showPaymentNotifications = true;

        [Header("Performance Tracking")]
        [SerializeField] private float efficiencyThreshold = 1.2f;      // Acima disso ganha bônus
        [SerializeField] private float inefficiencyThreshold = 0.6f;    // Abaixo disso paga penalidade

        // Estado interno
        private bool hasPaymentOccurredToday = false;
        private Dictionary<int, float> workerLastPayment = new Dictionary<int, float>();
        private Dictionary<WorkerType, WorkerCostData> costsByType;

        // Eventos
        public event Action<WorkerType, int> OnWorkerHired;
        public event Action<int, int> OnDailyPaymentMade;
        public event Action<int> OnInsufficientFundsForPayment;
        public event Action<int> OnWorkerFiredForNonPayment;

        public override void Awake()
        {
            base.Awake();
            InitializeCostDictionary();
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void InitializeCostDictionary()
        {
            costsByType = new Dictionary<WorkerType, WorkerCostData>
            {
                { WorkerType.Fazendeiro, fazendeiroCosts },
                { WorkerType.Repositor, repositorCosts },
                { WorkerType.Barista, baristaCosts }
            };
        }

        private void SubscribeToEvents()
        {
            // Integração com TimeManager
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTimeChanged += OnTimeChanged;
                TimeManager.Instance.OnResetDay += OnNewDay;
                TimeManager.Instance.OnCloseCoffeeShop += OnShopClosed;
            }

            // Integração com WorkerManager
            if (WorkerManager.Instance != null)
            {
                WorkerManager.Instance.OnWorkerHired += OnWorkerHiredByManager;
            }
        }

        #region Event Handlers
        private void OnTimeChanged(float currentHour)
        {
            // Verificar se chegou a hora do pagamento
            if (autoPayWorkers && !hasPaymentOccurredToday &&
                Mathf.FloorToInt(currentHour) >= Mathf.FloorToInt(paymentHour))
            {
                ProcessDailyPayments();
            }
        }

        private void OnNewDay()
        {
            hasPaymentOccurredToday = false;
            Debug.Log("[WorkerEconomics] Novo dia iniciado - resetando pagamentos");
        }

        private void OnShopClosed()
        {
            if (autoPayWorkers && !hasPaymentOccurredToday)
            {
                ProcessDailyPayments();
            }
        }

        private void OnWorkerHiredByManager(WorkerData worker)
        {
            workerLastPayment[worker.id] = TimeManager.Instance.CurrentHour;

            // Adicionar notificação de custo inicial
            int hiringCost = GetHiringCost(worker.type);
            ShopManager.Instance.SetMoney(ShopManager.Instance.GetMoney() - hiringCost);
            OnWorkerHired?.Invoke(worker.type, hiringCost);
        }
        #endregion

        #region Hiring System
        public bool CanAffordWorker(WorkerType type)
        {
            if (ShopManager.Instance == null) return false;

            int cost = GetHiringCost(type);
            return ShopManager.Instance.GetMoney() >= cost;
        }

        public bool TryHireWorker(WorkerType type)
        {
            if (!CanAffordWorker(type))
            {
                Debug.LogWarning($"Dinheiro insuficiente para contratar {type}. Necessário: ${GetHiringCost(type)}");
                return false;
            }

            int hiringCost = GetHiringCost(type);

            if (ShopManager.Instance.TrySpendMoney(hiringCost))
            {
                // O WorkerManager será notificado pelo evento e criará o trabalhador
                WorkerManager.Instance?.HireWorker(type);

                if (showPaymentNotifications)
                {
                    Debug.Log($"Trabalhador {type} contratado por ${hiringCost}");
                }

                return true;
            }

            return false;
        }

        public int GetHiringCost(WorkerType type)
        {
            return costsByType.TryGetValue(type, out var costs) ? costs.hiringCost : 100;
        }
        #endregion

        #region Daily Payment System
        public void ProcessDailyPayments()
        {
            if (hasPaymentOccurredToday) return;

            var activeWorkers = WorkerManager.Instance?.GetActiveWorkers() ?? new List<WorkerData>();

            if (activeWorkers.Count == 0)
            {
                hasPaymentOccurredToday = true;
                return;
            }

            int totalPayment = CalculateTotalDailyPayment(activeWorkers);

            if (ShopManager.Instance != null)
            {
                if (ShopManager.Instance.HasMoney(totalPayment))
                {
                    // Pagamento bem-sucedido
                    foreach (var worker in activeWorkers)
                    {
                        workerLastPayment[worker.id] = TimeManager.Instance.CurrentHour;
                    }

                    OnDailyPaymentMade?.Invoke(activeWorkers.Count, totalPayment);

                    if (showPaymentNotifications)
                    {
                        Debug.Log($"Pagamento diário realizado: ${totalPayment} para {activeWorkers.Count} trabalhadores");
                    }
                }
                else
                {
                    // Dinheiro insuficiente - processar consequências
                    HandleInsufficientFunds(activeWorkers, totalPayment);
                }
            }

            hasPaymentOccurredToday = true;
        }

        private void HandleInsufficientFunds(List<WorkerData> workers, int requiredAmount)
        {
            OnInsufficientFundsForPayment?.Invoke(requiredAmount);

            if (showPaymentNotifications)
            {
                Debug.LogWarning($"Dinheiro insuficiente para pagamento! Necessário: ${requiredAmount}, Disponível: ${ShopManager.Instance?.GetMoney() ?? 0}");
            }

            NotificationManager.Instance.BroadcastMessage(
                "Pagamento Insuficiente",
                $"Não foi possível pagar os trabalhadores hoje. Dinheiro insuficiente! Necessário: ${requiredAmount}, Disponível: ${ShopManager.Instance?.GetMoney() ?? 0}",
                SendMessageOptions.DontRequireReceiver
            );

            // Demitir trabalhadores menos eficientes se necessário
            if (ShouldFireWorkersForNonPayment())
            {
                FireLeastEfficientWorker(workers);
            }

            // Aplicar penalidades de moral/eficiência
            ApplyNonPaymentPenalties(workers);
        }

        private bool ShouldFireWorkersForNonPayment()
        {
            // Política: demitir apenas se já passou 2 dias sem pagamento
            return true; // Simplificado - na prática, implementar lógica mais complexa
        }

        private void FireLeastEfficientWorker(List<WorkerData> workers)
        {
            var leastEfficient = workers.OrderBy(w => w.efficiency).FirstOrDefault();
            if (leastEfficient.id != 0) // struct default check
            {
                WorkerManager.Instance?.FireWorker(leastEfficient.id);
                OnWorkerFiredForNonPayment?.Invoke(leastEfficient.id);

                if (showPaymentNotifications)
                {
                    Debug.Log($"Trabalhador {leastEfficient.type} (ID: {leastEfficient.id}) demitido por falta de pagamento");
                }
            }
        }

        private void ApplyNonPaymentPenalties(List<WorkerData> workers)
        {
            // Reduzir eficiência temporariamente
            // Isso seria implementado modificando os dados do trabalhador
            Debug.Log("Aplicando penalidades de moral por falta de pagamento");
        }
        #endregion

        #region Cost Calculations
        public int CalculateTotalDailyPayment(List<WorkerData> workers)
        {
            int total = 0;

            foreach (var worker in workers)
            {
                total += CalculateWorkerDailyPayment(worker);
            }

            return total;
        }

        public int CalculateWorkerDailyPayment(WorkerData worker)
        {
            if (!costsByType.TryGetValue(worker.type, out var costs))
                return 50; // Fallback

            int basePay = costs.GetTotalDailyCost();

            // Aplicar bônus/penalidades baseadas na performance
            if (worker.efficiency >= efficiencyThreshold)
            {
                basePay += costs.efficiencyBonus;
            }
            else if (worker.efficiency <= inefficiencyThreshold)
            {
                basePay += costs.inefficiencyPenalty; // Penalidade como custo adicional
            }

            return basePay;
        }

        public int GetDailyCost(WorkerType type)
        {
            return costsByType.TryGetValue(type, out var costs) ? costs.GetTotalDailyCost() : 35;
        }

        public int CalculateProjectedDailyCost()
        {
            var activeWorkers = WorkerManager.Instance?.GetActiveWorkers() ?? new List<WorkerData>();
            return CalculateTotalDailyPayment(activeWorkers);
        }
        #endregion

        #region Manual Payment Methods
        public bool TryPayWorkerManually(int workerID)
        {
            var worker = WorkerManager.Instance?.GetWorkerByID(workerID);
            if (worker == null) return false;

            int payment = CalculateWorkerDailyPayment(worker.Value);

            if (ShopManager.Instance != null && ShopManager.Instance.TrySpendMoney(payment))
            {
                workerLastPayment[workerID] = TimeManager.Instance.CurrentHour;

                if (showPaymentNotifications)
                {
                    Debug.Log($"Pagamento manual de ${payment} para trabalhador {worker.Value.type} (ID: {workerID})");
                }

                return true;
            }

            return false;
        }

        public void ForcePaymentProcess()
        {
            hasPaymentOccurredToday = false;
            ProcessDailyPayments();
        }
        #endregion

        #region Public Getters
        public bool HasPaymentOccurredToday() => hasPaymentOccurredToday;

        public float GetLastPaymentTime(int workerID)
        {
            return workerLastPayment.TryGetValue(workerID, out var time) ? time : -1f;
        }

        public string GetCostSummary()
        {
            var workers = WorkerManager.Instance?.GetActiveWorkers() ?? new List<WorkerData>();
            var dailyCost = CalculateTotalDailyPayment(workers);

            return $"Trabalhadores: {workers.Count} | Custo Diário: ${dailyCost}";
        }

        public Dictionary<WorkerType, int> GetCostBreakdown()
        {
            var breakdown = new Dictionary<WorkerType, int>();
            var workers = WorkerManager.Instance?.GetActiveWorkers() ?? new List<WorkerData>();

            foreach (var workerType in System.Enum.GetValues(typeof(WorkerType)).Cast<WorkerType>())
            {
                var workersOfType = workers.Where(w => w.type == workerType);
                breakdown[workerType] = workersOfType.Sum(w => CalculateWorkerDailyPayment(w));
            }

            return breakdown;
        }
        #endregion

        private void OnDestroy()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTimeChanged -= OnTimeChanged;
                TimeManager.Instance.OnResetDay -= OnNewDay;
                TimeManager.Instance.OnCloseCoffeeShop -= OnShopClosed;
            }

            if (WorkerManager.Instance != null)
            {
                WorkerManager.Instance.OnWorkerHired -= OnWorkerHiredByManager;
            }
        }

        #region Debug Methods
        [ContextMenu("Forçar Pagamento Diário")]
        private void DebugForcePayment()
        {
            ForcePaymentProcess();
        }

        [ContextMenu("Mostrar Resumo de Custos")]
        private void DebugShowCostSummary()
        {
            Debug.Log(GetCostSummary());
        }
        #endregion
    }
}