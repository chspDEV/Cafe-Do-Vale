using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;

namespace Tcp4
{
    /// <summary>
    /// Script de diagnóstico para identificar problemas com NPCs
    /// Adicione temporariamente ao NPC que está com problema
    /// </summary>
    public class NPCDiagnostic : MonoBehaviour
    {
        [Header("Auto Diagnosis")]
        [SerializeField] private bool runDiagnosisOnStart = true;
        [SerializeField] private bool logDetailedInfo = true;

        private NpcQuestInteractable npcInteractable;
        private BaseInteractable baseInteractable;

        private void Start()
        {
            if (runDiagnosisOnStart)
            {
                Invoke(nameof(RunFullDiagnosis), 0.5f); // Delay para garantir que tudo foi inicializado
            }
        }

        [ContextMenu("Run Full Diagnosis")]
        public void RunFullDiagnosis()
        {
            Debug.Log("=== STARTING FULL NPC DIAGNOSIS ===");

            CheckComponents();
            CheckGameAssets();
            CheckQuestManager();
            CheckNPCQuestStateManager();
            CheckMissions();
            CheckInteractableSystem();
            CheckIndicators();

            Debug.Log("=== DIAGNOSIS COMPLETE ===");
        }

        private void CheckComponents()
        {
            Debug.Log("--- Checking Components ---");

            npcInteractable = GetComponent<NpcQuestInteractable>();
            baseInteractable = GetComponent<BaseInteractable>();

            Debug.Log($"NpcQuestInteractable: {(npcInteractable != null ? "FOUND" : "MISSING")}");
            Debug.Log($"BaseInteractable: {(baseInteractable != null ? "FOUND" : "MISSING")}");

            if (npcInteractable != null)
            {
                Debug.Log($"NpcQuestInteractable Enabled: {npcInteractable.enabled}");
            }

            if (baseInteractable != null)
            {
                Debug.Log($"BaseInteractable Enabled: {baseInteractable.enabled}");
            }

            // Verifica outros componentes importantes
            var collider = GetComponent<Collider>();
            Debug.Log($"Collider: {(collider != null ? $"FOUND ({collider.GetType().Name}), IsTrigger: {collider.isTrigger}" : "MISSING")}");

            var rigidbody = GetComponent<Rigidbody>();
            Debug.Log($"Rigidbody: {(rigidbody != null ? "FOUND" : "NOT FOUND")}");
        }

        private void CheckGameAssets()
        {
            Debug.Log("--- Checking GameAssets ---");

            if (GameAssets.Instance != null)
            {
                Debug.Log("GameAssets.Instance: OK");

                if (GameAssets.Instance.player != null)
                {
                    Debug.Log($"GameAssets.Instance.player: OK ({GameAssets.Instance.player.name})");
                    Debug.Log($"Player Position: {GameAssets.Instance.player.transform.position}");
                }
                else
                {
                    Debug.LogError("GameAssets.Instance.player: NULL");

                    // Tenta encontrar o player de outras formas
                    GameObject playerByTag = GameObject.FindGameObjectWithTag("Player");
                    Debug.Log($"Player found by tag: {(playerByTag != null ? playerByTag.name : "NOT FOUND")}");
                }
            }
            else
            {
                Debug.LogError("GameAssets.Instance: NULL");
            }
        }

        private void CheckQuestManager()
        {
            Debug.Log("--- Checking QuestManager ---");

            if (QuestManager.Instance != null)
            {
                Debug.Log("QuestManager.Instance: OK");
                Debug.Log($"Current Mission: {(QuestManager.Instance.CurrentMission != null ? QuestManager.Instance.CurrentMission.questName : "NULL")}");
                Debug.Log($"Completed Tutorials Count: {QuestManager.Instance.completedTutorials.Count}");

                var missions = QuestManager.Instance.GetTutorialMissions();
                Debug.Log($"Total Tutorial Missions: {(missions != null ? missions.Count : 0)}");
            }
            else
            {
                Debug.LogError("QuestManager.Instance: NULL");
            }
        }

        private void CheckNPCQuestStateManager()
        {
            Debug.Log("--- Checking NPCQuestStateManager ---");

            if (NPCQuestStateManager.Instance != null)
            {
                Debug.Log("NPCQuestStateManager.Instance: OK");
            }
            else
            {
                Debug.LogError("NPCQuestStateManager.Instance: NULL");

                // Tenta encontrar manualmente
                var manager = FindFirstObjectByType<NPCQuestStateManager>();
                Debug.Log($"NPCQuestStateManager found by search: {(manager != null ? "FOUND" : "NOT FOUND")}");
            }
        }

        private void CheckMissions()
        {
            Debug.Log("--- Checking Missions ---");

            if (npcInteractable != null)
            {
                if (npcInteractable.missions != null)
                {
                    Debug.Log($"Missions Count: {npcInteractable.missions.Count}");

                    for (int i = 0; i < npcInteractable.missions.Count; i++)
                    {
                        var mission = npcInteractable.missions[i];
                        if (mission != null)
                        {
                            Debug.Log($"Mission {i}: {mission.questName} (ID: {mission.questID})");
                            Debug.Log($"  Steps: {(mission.steps != null ? mission.steps.Count : 0)}");
                            Debug.Log($"  Completed: {mission.isCompleted}");
                            Debug.Log($"  Started: {mission.isStarted}");
                        }
                        else
                        {
                            Debug.LogError($"Mission {i}: NULL");
                        }
                    }
                }
                else
                {
                    Debug.LogError("Missions list: NULL");
                }

                if (npcInteractable.dialogues != null)
                {
                    Debug.Log($"Dialogues Count: {npcInteractable.dialogues.Count}");
                }
                else
                {
                    Debug.LogError("Dialogues list: NULL");
                }
            }
        }

        private void CheckInteractableSystem()
        {
            Debug.Log("--- Checking Interactable System ---");

            // Simula OnFocus
            try
            {
                Debug.Log("Testing OnFocus...");
                if (npcInteractable != null)
                {
                    npcInteractable.OnFocus();
                    Debug.Log("OnFocus: SUCCESS");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"OnFocus ERROR: {e.Message}");
            }

            // Simula OnLostFocus
            try
            {
                Debug.Log("Testing OnLostFocus...");
                if (npcInteractable != null)
                {
                    npcInteractable.OnLostFocus();
                    Debug.Log("OnLostFocus: SUCCESS");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"OnLostFocus ERROR: {e.Message}");
            }
        }

        private void CheckIndicators()
        {
            Debug.Log("--- Checking Indicators ---");

            if (npcInteractable != null)
            {
                // Acessa os campos privados usando reflection para diagnóstico
                var questIndicatorField = typeof(NpcQuestInteractable).GetField("questAvailableIndicator",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (questIndicatorField != null)
                {
                    var questIndicator = questIndicatorField.GetValue(npcInteractable) as GameObject;
                    Debug.Log($"Quest Available Indicator: {(questIndicator != null ? $"FOUND ({questIndicator.name})" : "NULL")}");

                    if (questIndicator != null)
                    {
                        Debug.Log($"  Active: {questIndicator.activeSelf}");
                        Debug.Log($"  Position: {questIndicator.transform.position}");
                    }
                }

                var visualIndicatorField = typeof(NpcQuestInteractable).GetField("visualIndicatorComponent",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (visualIndicatorField != null)
                {
                    var visualIndicator = visualIndicatorField.GetValue(npcInteractable) as QuestVisualIndicator;
                    Debug.Log($"Visual Indicator Component: {(visualIndicator != null ? "FOUND" : "NULL")}");
                }

                // Força atualização do indicador
                try
                {
                    npcInteractable.ForceUpdateIndicator();
                    Debug.Log("Force Update Indicator: SUCCESS");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Force Update Indicator ERROR: {e.Message}");
                }
            }
        }

        [ContextMenu("Test Interaction")]
        public void TestInteraction()
        {
            Debug.Log("=== TESTING INTERACTION ===");

            if (npcInteractable != null)
            {
                try
                {
                    npcInteractable.OnInteract();
                    Debug.Log("Test Interaction: SUCCESS");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Test Interaction ERROR: {e.Message}");
                }
            }
            else
            {
                Debug.LogError("Cannot test interaction - NpcQuestInteractable not found!");
            }
        }

        [ContextMenu("Fix Common Issues")]
        public void FixCommonIssues()
        {
            Debug.Log("=== ATTEMPTING TO FIX COMMON ISSUES ===");

            // 1. Verifica se NPCQuestStateManager existe
            if (NPCQuestStateManager.Instance == null)
            {
                Debug.Log("Creating NPCQuestStateManager...");
                GameObject managerObj = new GameObject("NPCQuestStateManager");
                managerObj.AddComponent<NPCQuestStateManager>();
                DontDestroyOnLoad(managerObj);
            }

            // 2. Verifica se o NPC tem ID
            if (npcInteractable != null)
            {
                string currentID = npcInteractable.GetNPCID();
                if (string.IsNullOrEmpty(currentID))
                {
                    string newID = gameObject.name + "_" + GetInstanceID();
                    npcInteractable.SetNPCID(newID);
                    Debug.Log($"Set NPC ID to: {newID}");
                }
            }

            // 3. Força refresh do estado
            if (npcInteractable != null)
            {
                npcInteractable.RefreshNPCState();
                Debug.Log("Refreshed NPC State");
            }

            // 4. Verifica collider
            var collider = GetComponent<Collider>();
            if (collider == null)
            {
                Debug.Log("Adding BoxCollider...");
                var boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
                boxCollider.size = Vector3.one * 2f; // Tamanho padrão
            }

            // 5. Cria indicador básico se não existir
            if (npcInteractable != null && npcInteractable.questAvailableIndicator == null)
            {
                Debug.Log("Creating basic quest indicator...");
                CreateBasicIndicator();
            }

            Debug.Log("=== FIX ATTEMPT COMPLETE ===");
        }

        private void CreateBasicIndicator()
        {
            // Cria um indicador básico
            GameObject indicator = new GameObject("QuestIndicator");
            indicator.transform.SetParent(transform);
            indicator.transform.localPosition = Vector3.up * 2f;

            // Adiciona um cubo simples como indicador
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(indicator.transform);
            cube.transform.localScale = Vector3.one * 0.3f;
            cube.transform.localPosition = Vector3.zero;

            // Remove o collider do cubo
            var cubeCollider = cube.GetComponent<Collider>();
            if (cubeCollider != null)
                DestroyImmediate(cubeCollider);

            // Muda a cor para amarelo
            var renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.yellow;
            }

            // Adiciona o componente QuestVisualIndicator
            var visualIndicator = indicator.AddComponent<QuestVisualIndicator>();

            // Configura para usar o cubo como elemento visual
            visualIndicator.visualElements = new GameObject[] { cube };

            indicator.SetActive(false); // Começa desativado

            Debug.Log($"Created basic indicator: {indicator.name}");
        }

        // Método público para acessar o questAvailableIndicator (para o diagnostic)
        public GameObject GetQuestIndicator()
        {
            if (npcInteractable != null)
            {
                var questIndicatorField = typeof(NpcQuestInteractable).GetField("questAvailableIndicator",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (questIndicatorField != null)
                {
                    return questIndicatorField.GetValue(npcInteractable) as GameObject;
                }
            }
            return null;
        }

        [ContextMenu("Create Test Mission")]
        public void CreateTestMission()
        {
            if (npcInteractable != null && npcInteractable.missions.Count == 0)
            {
                Debug.Log("NPC has no missions. You need to create a Quest ScriptableObject and assign it manually.");
                Debug.Log("Right-click in Project -> Create -> ScriptableObjects -> Quest");
            }
            else if (npcInteractable != null)
            {
                Debug.Log($"NPC already has {npcInteractable.missions.Count} mission(s) configured.");
            }
        }
    }
}