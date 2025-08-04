// ===== WorkerPerformanceMonitor.cs (Novo - Para Monitoramento de Performance) =====
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Tcp4.Assets.Resources.Scripts.Managers;

namespace Tcp4.Performance
{
    public class WorkerPerformanceMonitor : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private float monitoringInterval = 5f;
        [SerializeField] private int maxHistoryEntries = 100;

        private Dictionary<int, WorkerPerformanceData> workerPerformance = new();
        private float lastMonitorTime;

        [System.Serializable]
        public class WorkerPerformanceData
        {
            public int workerID;
            public WorkerType type;
            public float averageTaskTime;
            public int tasksCompleted;
            public int tasksFailed;
            public float efficiency;
            public List<float> taskTimeHistory = new();
        }

        private void Update()
        {
            if (Time.time - lastMonitorTime >= monitoringInterval)
            {
                UpdatePerformanceData();
                lastMonitorTime = Time.time;
            }
        }

        private void UpdatePerformanceData()
        {
            if (WorkerManager.Instance == null) return;

            var activeWorkers = WorkerManager.Instance.GetActiveWorkers();

            foreach (var worker in activeWorkers)
            {
                if (!workerPerformance.ContainsKey(worker.id))
                {
                    workerPerformance[worker.id] = new WorkerPerformanceData
                    {
                        workerID = worker.id,
                        type = worker.type
                    };
                }

                var perfData = workerPerformance[worker.id];
                perfData.tasksCompleted = worker.tasksCompleted;
                perfData.tasksFailed = worker.tasksFailed;
                perfData.efficiency = worker.efficiency;

                // Calcular eficiência baseada em tarefas completadas vs falhadas
                if (perfData.tasksCompleted + perfData.tasksFailed > 0)
                {
                    float successRate = (float)perfData.tasksCompleted / (perfData.tasksCompleted + perfData.tasksFailed);
                    perfData.efficiency = successRate * worker.efficiency;
                }
            }
        }

        public WorkerPerformanceData GetWorkerPerformance(int workerID)
        {
            return workerPerformance.TryGetValue(workerID, out var data) ? data : null;
        }

        public List<WorkerPerformanceData> GetTopPerformers(int count = 5)
        {
            return workerPerformance.Values
                .OrderByDescending(w => w.efficiency)
                .Take(count)
                .ToList();
        }

        public List<WorkerPerformanceData> GetWorkersByType(WorkerType type)
        {
            return workerPerformance.Values
                .Where(w => w.type == type)
                .OrderByDescending(w => w.efficiency)
                .ToList();
        }

        public float GetAverageEfficiencyByType(WorkerType type)
        {
            var workers = GetWorkersByType(type);
            return workers.Count > 0 ? workers.Average(w => w.efficiency) : 0f;
        }
    }
}