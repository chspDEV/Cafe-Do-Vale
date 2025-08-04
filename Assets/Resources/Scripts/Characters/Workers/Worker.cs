using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;
using System.Collections;

namespace Tcp4
{
    public class Worker : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private Transform modelAnchor; // Ponto para ancorar o modelo visual
        public Animator ModelAnimator { get; private set; }

        [Header("UI References")]
        [SerializeField] private Canvas workerCanvas;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private NavMeshAgent NavMeshAgent;

        [Header("Debug UI")]
        [SerializeField] private TextMeshProUGUI debugText;
        [SerializeField] private Image debugBackground;

        private WorkerData workerData;
        private float debugUpdateTimer = 0f;
        private const float DEBUG_UPDATE_INTERVAL = 0.5f; // Atualizar debug a cada 0.5 segundos

        private void Update()
        {
            if (ModelAnimator != null)
            {
                ModelAnimator.SetFloat("Speed", NavMeshAgent.velocity.magnitude);
            }

            // Atualizar debug periodicamente
            debugUpdateTimer += Time.deltaTime;
            if (debugUpdateTimer >= DEBUG_UPDATE_INTERVAL)
            {
                UpdateDebugDisplay();
                debugUpdateTimer = 0f;
            }
        }

        public void Setup(WorkerData data)
        {
            workerData = data;

            if (workerCanvas != null)
                workerCanvas.worldCamera = Camera.main;

            SetupDebugUI();
            UpdateDebugDisplay();
        }

        private void SetupDebugUI()
        {
            // Se não há debugText, criar um
            if (debugText == null && workerCanvas != null)
            {
                CreateDebugUI();
            }
        }

        private void CreateDebugUI()
        {
            // Criar background para o debug
            GameObject debugBG = new GameObject("DebugBackground");
            debugBG.transform.SetParent(workerCanvas.transform, false);

            debugBackground = debugBG.AddComponent<Image>();
            debugBackground.color = new Color(0, 0, 0, 0.7f);

            RectTransform bgRect = debugBackground.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 1);
            bgRect.anchorMax = new Vector2(0, 1);
            bgRect.pivot = new Vector2(0, 1);
            bgRect.anchoredPosition = new Vector2(10, -10);
            bgRect.sizeDelta = new Vector2(200, 120);

            // Criar texto de debug
            GameObject debugTextGO = new GameObject("DebugText");
            debugTextGO.transform.SetParent(debugBG.transform, false);

            debugText = debugTextGO.AddComponent<TextMeshProUGUI>();
            debugText.text = "Debug Info";
            debugText.fontSize = 10;
            debugText.color = Color.white;
            debugText.alignment = TextAlignmentOptions.TopLeft;

            RectTransform textRect = debugText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 5);
            textRect.offsetMax = new Vector2(-5, -5);
        }

        private void UpdateDebugDisplay()
        {
            if (debugText == null) return;

            string debugInfo = GetDebugInfo();
            debugText.text = debugInfo;

            // Mudar cor do background baseado no estado
            if (debugBackground != null)
            {
                Color bgColor = GetStateColor(workerData.currentState);
                debugBackground.color = new Color(bgColor.r, bgColor.g, bgColor.b, 0.7f);
            }
        }

        private string GetDebugInfo()
        {
            string info = $"<b>Worker #{workerData.id}</b>\n";
            info += $"<b>Type:</b> {workerData.type}\n";
            info += $"<b>State:</b> {workerData.currentState}\n";
            info += $"<b>Task ID:</b> {(workerData.currentTaskID == -1 ? "None" : workerData.currentTaskID.ToString())}\n";
            info += $"<b>Active:</b> {(workerData.isActive ? "Yes" : "No")}\n";
            info += $"<b>Hired:</b> {(workerData.isHired ? "Yes" : "No")}\n";
            info += $"<b>Carrying:</b> {workerData.inventoryCount} items\n";
            info += $"<b>Efficiency:</b> {workerData.efficiency:F2}\n";

            // Informações de movimento
            if (NavMeshAgent != null)
            {
                info += $"<b>Speed:</b> {NavMeshAgent.velocity.magnitude:F1}\n";
                info += $"<b>Moving:</b> {(NavMeshAgent.velocity.magnitude > 0.1f ? "Yes" : "No")}";
            }

            return info;
        }

        private Color GetStateColor(WorkerState state)
        {
            switch (state)
            {
                case WorkerState.Idle:
                    return Color.yellow;
                case WorkerState.MovingToOrigin:
                case WorkerState.MovingToWorkstation:
                case WorkerState.MovingToDestination:
                    return Color.blue;
                case WorkerState.CollectingItem:
                case WorkerState.Working:
                case WorkerState.DeliveringItem:
                    return Color.green;
                case WorkerState.Resting:
                    return Color.cyan;
                case WorkerState.GoingHome:
                    return Color.cyan;
                case WorkerState.OffDuty:
                    return Color.gray;
                default:
                    return Color.white;
            }
        }

        public void SetVisualModel(GameObject modelPrefab)
        {
            if (modelAnchor.childCount > 0)
            {
                Destroy(modelAnchor.GetChild(0).gameObject);
            }

            GameObject modelInstance = Instantiate(modelPrefab, modelAnchor);
            ModelAnimator = modelInstance.GetComponent<Animator>();

            // Forçar esperar 1 frame para garantir animação
            StartCoroutine(EnableAgentNextFrame());
        }

        private IEnumerator EnableAgentNextFrame()
        {
            NavMeshAgent.enabled = false;
            yield return null;
            NavMeshAgent.enabled = true;
        }


        public void SetWorkingAnimation(bool isWorking)
        {
            if (ModelAnimator != null)
            {
                ModelAnimator.SetBool("IsWorking", isWorking);
            }
        }

        public void UpdateWorkerData(WorkerData newData)
        {
            workerData = newData;
        }

        // Método para debug manual no console
        [ContextMenu("Log Debug Info")]
        public void LogDebugInfo()
        {
            Debug.Log($"[WORKER DEBUG] {GetDebugInfo().Replace("<b>", "").Replace("</b>", "").Replace("\n", " | ")}");
        }
    }
}