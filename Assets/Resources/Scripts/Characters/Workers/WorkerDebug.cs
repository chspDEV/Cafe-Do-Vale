using System.Linq;
using Tcp4.Assets.Resources.Scripts.Managers;

using UnityEngine;

namespace Tcp4
{
    public class WorkerDebug : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool showWorkerPaths = true;
        [SerializeField] private bool showTaskInfo = true;
        [SerializeField] private bool logWorkerActions = false;

        [Header("Test Controls")]
        [SerializeField] private KeyCode testHarvestKey = KeyCode.H;
        [SerializeField] private KeyCode testRefineKey = KeyCode.R;
        [SerializeField] private KeyCode testDrinkKey = KeyCode.D;
        [SerializeField] private KeyCode moneyKey = KeyCode.Keypad1;

        private void Update()
        {
            HandleTestInputs();
        }

        private void HandleTestInputs()
        {
            if (Input.GetKeyDown(testHarvestKey))
            {
                TestCreateHarvestTask();
            }

            if (Input.GetKeyDown(testRefineKey))
            {
                TestCreateRefineTask();
            }

            if (Input.GetKeyDown(testDrinkKey))
            {
                TestCreateDrinkTask();
            }

            if (Input.GetKeyDown(moneyKey)) ShopManager.Instance.AddMoney(1000);
        }

        private void TestCreateHarvestTask()
        {
            // Criar uma tarefa de teste para colheita
            if (WorkerManager.Instance != null)
            {
                var productionAreas = FindObjectsByType<ProductionArea>(FindObjectsSortMode.None);
                if (productionAreas.Length > 0)
                {
                    var testProduct = ScriptableObject.CreateInstance<BaseProduct>();
                    testProduct.productName = "Test Crop";
                    testProduct.productID = 99;

                    WorkerManager.Instance.CreateHarvestTask(productionAreas[0].areaID, testProduct);
                    Debug.Log("Tarefa de colheita de teste criada!");
                }
            }
        }

        private void TestCreateRefineTask()
        {
            // Criar uma tarefa de teste para refinamento
            if (WorkerManager.Instance != null)
            {
                var rawProduct = ScriptableObject.CreateInstance<BaseProduct>();
                rawProduct.productName = "Raw Material";
                rawProduct.productID = 100;

                var refinedProduct = ScriptableObject.CreateInstance<BaseProduct>();
                refinedProduct.productName = "Refined Material";
                refinedProduct.productID = 101;

                WorkerManager.Instance.CreateRefineTask(rawProduct, refinedProduct);
                Debug.Log("Tarefa de refinamento de teste criada!");
            }
        }

        private void TestCreateDrinkTask()
        {
            // Criar uma tarefa de teste para bebida
            if (WorkerManager.Instance != null)
            {
                var testDrink = ScriptableObject.CreateInstance<Drink>();
                testDrink.productName = "Test Coffee";
                testDrink.productID = 102;
                testDrink.preparationTime = 30f;

                WorkerManager.Instance.CreateDrinkTask(testDrink, UnityEngine.Random.Range(1000, 9999));
                Debug.Log("Tarefa de bebida de teste criada!");
            }
        }

        private void OnGUI()
        {
            if (!showTaskInfo) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label("=== Worker System Debug ===");

            if (WorkerManager.Instance != null)
            {
                var workers = WorkerManager.Instance.GetActiveWorkers();
                var pendingTasks = WorkerManager.Instance.GetPendingTasks();
                var activeTasks = WorkerManager.Instance.GetActiveTasks();

                GUILayout.Label($"Trabalhadores Ativos: {workers.Count}");
                GUILayout.Label($"Tarefas Pendentes: {pendingTasks.Count}");
                GUILayout.Label($"Tarefas Ativas: {activeTasks.Count}");

                GUILayout.Space(10);
                GUILayout.Label("=== Trabalhadores por Tipo ===");

                var farmers = workers.Count(w => w.type == WorkerType.Fazendeiro);
                var repositors = workers.Count(w => w.type == WorkerType.Repositor);
                var baristas = workers.Count(w => w.type == WorkerType.Barista);

                GUILayout.Label($"Fazendeiros: {farmers}");
                GUILayout.Label($"Repositores: {repositors}");
                GUILayout.Label($"Baristas: {baristas}");

                GUILayout.Space(10);
                GUILayout.Label("=== Controles de Teste ===");
                GUILayout.Label($"[{testHarvestKey}] - Criar Tarefa de Colheita");
                GUILayout.Label($"[{testRefineKey}] - Criar Tarefa de Refinamento");
                GUILayout.Label($"[{testDrinkKey}] - Criar Tarefa de Bebida");

                if (GUILayout.Button("Contratar Fazendeiro"))
                {
                    WorkerManager.Instance.HireWorker(WorkerType.Fazendeiro);
                }

                if (GUILayout.Button("Contratar Repositor"))
                {
                    WorkerManager.Instance.HireWorker(WorkerType.Repositor);
                }

                if (GUILayout.Button("Contratar Barista"))
                {
                    WorkerManager.Instance.HireWorker(WorkerType.Barista);
                }
            }
            else
            {
                GUILayout.Label("WorkerManager não encontrado!");
            }

            GUILayout.EndArea();
        }

        private void OnDrawGizmos()
        {
            if (!showWorkerPaths || WorkerManager.Instance == null) return;

            var workers = FindObjectsByType<Worker>(FindObjectsSortMode.None);
            foreach (var worker in workers)
            {
                // Desenhar caminho do trabalhador
                var agent = worker.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null && agent.hasPath)
                {
                    Gizmos.color = GetWorkerColor(worker);
                    var path = agent.path;

                    for (int i = 0; i < path.corners.Length - 1; i++)
                    {
                        Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
                    }

                    // Desenhar destino
                    Gizmos.DrawWireSphere(agent.destination, 0.5f);
                }
            }
        }

        private Color GetWorkerColor(Worker worker)
        {
            // Assumindo que há uma forma de identificar o tipo do worker
            // Na implementação real, você acessaria worker.GetWorkerType() ou similar
            return Color.white; // Placeholder
        }
    }
}