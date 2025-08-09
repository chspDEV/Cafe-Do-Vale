using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using Tcp4.Assets.Resources.Scripts.Managers;

namespace Tcp4
{
    public class NpcQuestInteractable : BaseInteractable
    {
        [Header("Quest Configuration")]
        public List<Quest> missions; // Lista de ScriptableObjects de miss�o
        public bool showDebugMessages = true;

        [Header("Animation")]
        public Animator anim;
        public string onLookAnimation;
        public string onInteractAnimation;

        [Header("Camera")]
        public CinemachineCamera npcCamera;

        [Header("Dialogue")]
        public List<DialogueData> dialogues;

        [Header("Visual Indicators")]
        [SerializeField] private IndicatorSetupType indicatorSetup = IndicatorSetupType.ChildObject;
        [SerializeField] public GameObject questAvailableIndicator; // Indicador quando h� miss�o dispon�vel (GameObject filho ou prefab)
        [SerializeField] private GameObject questIndicatorPrefab; // Prefab do indicador (usado se indicatorSetup = Prefab)
        [SerializeField] private QuestVisualIndicator visualIndicatorComponent; // Componente de indicador avan�ado
        [SerializeField] private Transform indicatorParent; // Onde o indicador ser� posicionado (geralmente acima da cabe�a)
        [SerializeField] private Vector3 indicatorOffset = Vector3.up * 2f; // Offset do indicador

        [Header("NPC Identity")]
        [SerializeField] private string npcID; // ID �nico do NPC para persist�ncia

        public enum IndicatorSetupType
        {
            ChildObject,    // GameObject j� existe como filho do NPC
            Prefab,         // Instancia um prefab quando necess�rio
            UIElement       // Para indicadores na UI (futuro)
        }

        private int currentMissionIndex = -1;
        private GameObject player;
        private Quaternion idleRotation;

        // Estados do NPC para controle de di�logo - agora gerenciados pelo NPCQuestStateManager
        private NPCQuestState npcState;

        public override void Start()
        {
            base.Start();
            InitializeNPC();
        }

        private void InitializeNPC()
        {
            if (GameAssets.Instance != null && GameAssets.Instance.player != null)
            {
                player = GameAssets.Instance.player;
            }
            else
            {
                Debug.LogWarning($"[NPC {gameObject.name}] GameAssets.Instance.player is null! Trying to find player...");
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    player = playerObj;
            }

            idleRotation = transform.rotation;

            // Inicializa ID do NPC se n�o estiver definido
            if (string.IsNullOrEmpty(npcID))
            {
                npcID = gameObject.name + "_" + GetInstanceID();
                Debug.LogWarning($"NPC ID n�o definido para {gameObject.name}. Usando ID gerado: {npcID}");
            }

            // Carrega estado persistente do NPC - com fallback
            try
            {
                LoadNPCState();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading NPC state for {npcID}: {e.Message}");
                // Cria estado padr�o se houve erro
                npcState = new NPCQuestState(npcID);
            }

            UpdateCurrentMissionIndex();
            UpdateVisualIndicator();

            // Se n�o h� indicatorParent definido, usa o pr�prio transform do NPC
            if (indicatorParent == null)
                indicatorParent = transform;

            // Inicializa componente de indicador visual se existir
            if (visualIndicatorComponent == null)
                visualIndicatorComponent = GetComponentInChildren<QuestVisualIndicator>();

            if (showDebugMessages)
            {
                Debug.Log($"[NPC {npcID}] Initialized - Current Mission Index: {currentMissionIndex}, Total Missions: {(missions != null ? missions.Count : 0)}");
            }
        }

        private void LoadNPCState()
        {
            if (NPCQuestStateManager.Instance != null)
            {
                npcState = NPCQuestStateManager.Instance.GetNPCState(npcID);

                if (npcState != null)
                {
                    currentMissionIndex = npcState.currentMissionIndex;
                }
            }
            else
            {
                Debug.LogWarning($"[NPC {npcID}] NPCQuestStateManager.Instance is null! Creating default state...");
                npcState = new NPCQuestState(npcID);
            }
        }

        private void SaveNPCState()
        {
            if (npcState != null && NPCQuestStateManager.Instance != null)
            {
                npcState.currentMissionIndex = currentMissionIndex;
                NPCQuestStateManager.Instance.UpdateNPCState(npcID, npcState);
            }
        }

        public override void Update()
        {
            base.Update();
            // Verifica se o status das miss�es mudou para atualizar o indicador
            CheckForMissionStatusChanges();
        }

        private void CheckForMissionStatusChanges()
        {
            if (QuestManager.Instance.completedTutorials.Count <= 0) return;

            int previousIndex = currentMissionIndex;
            UpdateCurrentMissionIndex();

            if (previousIndex != currentMissionIndex)
            {
                UpdateVisualIndicator();
                SaveNPCState(); // Salva mudan�as de estado

                // Reset do estado se mudou de miss�o
                if (currentMissionIndex < missions.Count && currentMissionIndex >= 0)
                {
                    string newMissionID = missions[currentMissionIndex].questID;
                    if (newMissionID != npcState.lastStartedMissionID)
                    {
                        npcState.hasStartedCurrentMission = false;
                        SaveNPCState();
                    }
                }
            }
        }

        public override void OnFocus()
        {
            // Remove esta verifica��o que pode estar bloqueando
            if (QuestManager.Instance.completedTutorials.Count <= 0) return;

            base.OnFocus();

            if (anim != null && !string.IsNullOrEmpty(onLookAnimation))
                anim.Play(onLookAnimation);

            // Rotaciona para olhar o jogador
            if (player != null)
            {
                Vector3 direction = player.transform.position - transform.position;
                direction.y = 0; // Mant�m apenas a rota��o horizontal
                transform.rotation = Quaternion.LookRotation(direction * -1f);
            }
        }

        public override void OnLostFocus()
        {
            // Remove esta verifica��o que pode estar bloqueando
            // if (QuestManager.Instance.completedTutorials.Count <= 0) return;

            base.OnLostFocus();
            transform.rotation = idleRotation;
        }

        private void UpdateCurrentMissionIndex()
        {
            currentMissionIndex = -1;



            for (int i = 0; i < missions.Count; i++)
            {
                string questId = missions[i].questID;
                bool missionCompleted = QuestManager.Instance.IsMissionCompleted(questId);

                if (!missionCompleted)
                {
                    currentMissionIndex = i;
                    break;
                }
            }

            // Se todas estiverem completas
            if (currentMissionIndex == -1)
            {
                currentMissionIndex = missions.Count;
            }
        }

        private void UpdateVisualIndicator()
        {
            bool shouldShowIndicator = HasAvailableMission() && (npcState?.hasStartedCurrentMission != true);

            if (showDebugMessages)
            {
                Debug.Log($"[NPC {npcID}] UpdateVisualIndicator - Should Show: {shouldShowIndicator}, Has Available: {HasAvailableMission()}, Has Started: {npcState?.hasStartedCurrentMission}");
            }

            // Atualiza indicador simples (GameObject)
            if (questAvailableIndicator != null)
            {
                if (questAvailableIndicator.activeSelf != shouldShowIndicator)
                {
                    questAvailableIndicator.SetActive(shouldShowIndicator);

                    if (shouldShowIndicator)
                    {
                        // Posiciona o indicador
                        questAvailableIndicator.transform.position = indicatorParent.position + indicatorOffset;
                        questAvailableIndicator.transform.SetParent(indicatorParent);
                    }
                }
            }
            else if (showDebugMessages && shouldShowIndicator)
            {
                Debug.LogWarning($"[NPC {npcID}] questAvailableIndicator is null but should show indicator!");
            }

            // Atualiza indicador avan�ado (QuestVisualIndicator component)
            if (visualIndicatorComponent != null)
            {
                visualIndicatorComponent.SetActive(shouldShowIndicator);
            }
        }

        private bool HasAvailableMission()
        {
            if (missions == null || currentMissionIndex >= missions.Count || currentMissionIndex < 0)
            {
                if (showDebugMessages)
                    Debug.Log($"[NPC {npcID}] HasAvailableMission = false - Index: {currentMissionIndex}, Count: {(missions != null ? missions.Count : 0)}");
                return false;
            }

            Quest currentMission = missions[currentMissionIndex];
            if (currentMission == null)
            {
                if (showDebugMessages)
                    Debug.LogWarning($"[NPC {npcID}] Current mission is null at index {currentMissionIndex}");
                return false;
            }

            bool missionCompleted = QuestManager.Instance.IsMissionCompleted(currentMission.questID);
            bool missionStarted = QuestManager.Instance.IsMissionStarted(currentMission.questID);

            bool hasAvailable = !missionCompleted && !missionStarted;

            if (showDebugMessages)
            {
                Debug.Log($"[NPC {npcID}] Mission '{currentMission.questName}' - Completed: {missionCompleted}, Started: {missionStarted}, Available: {hasAvailable}");
            }

            return hasAvailable;
        }

        public override void OnInteract()
        {
            // Remove esta verifica��o que pode estar bloqueando
            // if (QuestManager.Instance.completedTutorials.Count <= 0) return;

            base.OnInteract();

            // Debug para verificar o que est� acontecendo
            if (showDebugMessages)
            {
                Debug.Log($"[NPC {npcID}] OnInteract called - Current Mission Index: {currentMissionIndex}");
                Debug.Log($"[NPC {npcID}] Total Missions: {missions.Count}");
                if (npcState != null)
                {
                    Debug.Log($"[NPC {npcID}] NPC State - Has Started: {npcState.hasStartedCurrentMission}");
                }
            }

            // Todas as miss�es completas
            if (currentMissionIndex >= missions.Count)
            {
                HandleAllMissionsCompleted();
                return;
            }

            // Verifica se temos miss�es v�lidas
            if (missions == null || missions.Count == 0)
            {
                if (showDebugMessages) Debug.LogWarning($"[NPC {npcID}] No missions configured!");
                return;
            }

            Quest currentMission = missions[currentMissionIndex];
            if (currentMission == null)
            {
                if (showDebugMessages) Debug.LogError($"[NPC {npcID}] Current mission is null at index {currentMissionIndex}");
                return;
            }

            bool missionCompleted = QuestManager.Instance.IsMissionCompleted(currentMission.questID);
            bool missionStarted = QuestManager.Instance.IsMissionStarted(currentMission.questID);

            if (showDebugMessages)
            {
                Debug.Log($"[NPC {npcID}] Mission '{currentMission.questName}' - Completed: {missionCompleted}, Started: {missionStarted}");
            }

            if (missionCompleted)
            {
                HandleCompletedMission();
            }
            else if (!missionStarted || !npcState.hasStartedCurrentMission)
            {
                // Garante que o di�logo s� inicie se ambos os estados estiverem ok
                StartCurrentMission();
            }
            else
            {
                HandleMissionInProgress(currentMission);
            }

        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Debug Tools/Resetar Miss�o do NPC Selecionado")]
        public static void ResetarNPCSelecionado()
        {
            if (UnityEditor.Selection.activeGameObject.TryGetComponent(out NpcQuestInteractable npc))
            {
                npc.SetMissionStartedState(false);
                Debug.Log("Miss�o resetada via menu para: " + npc.name);
            }
            else
            {
                Debug.LogWarning("Nenhum NPC selecionado com NpcQuestInteractable!");
            }
        }
#endif

        private void HandleAllMissionsCompleted()
        {
            if (showDebugMessages)
                Debug.Log("Todas as miss�es deste NPC foram completadas!");

            // Aqui voc� pode adicionar um di�logo especial para quando todas as miss�es est�o completas
            // Por exemplo, um di�logo de agradecimento
        }

        private void HandleCompletedMission()
        {
            // Avan�a para pr�xima miss�o
            currentMissionIndex++;
            UpdateCurrentMissionIndex();

            // Reset do estado para nova miss�o
            npcState.hasStartedCurrentMission = false;
            SaveNPCState();

            if (currentMissionIndex < missions.Count)
            {
                StartCurrentMission();
            }
            else
            {
                UpdateVisualIndicator(); // Remove indicador se n�o h� mais miss�es
            }
        }

        private void HandleMissionInProgress(Quest currentMission)
        {
            if (showDebugMessages)
                Debug.Log($"Miss�o em progresso: {currentMission.questName}. Di�logo n�o pode ser repetido.");

            // N�o inicia di�logo novamente - miss�o j� foi aceita
            // Opcional: Voc� pode mostrar uma mensagem r�pida ou som de "n�o dispon�vel"
        }

        private void StartCurrentMission()
        {
            if (currentMissionIndex >= missions.Count) return;

            Quest currentMission = missions[currentMissionIndex];

            // Marca que esta miss�o foi iniciada pelo di�logo usando o sistema persistente
            NPCQuestStateManager.Instance.SetNPCMissionStarted(npcID, currentMission.questID, currentMissionIndex);
            npcState = NPCQuestStateManager.Instance.GetNPCState(npcID); // Atualiza refer�ncia local

            // Registra no QuestManager como iniciada
            // Atualiza estado do NPC antes de iniciar a miss�o
            NPCQuestStateManager.Instance.SetNPCMissionStarted(npcID, currentMission.questID, currentMissionIndex);
            npcState = NPCQuestStateManager.Instance.GetNPCState(npcID); // Reatualiza local

            // S� ent�o inicia a miss�o no QuestManager
            QuestManager.Instance.StartMission(currentMission.questID);


            if (showDebugMessages)
                Debug.Log($"Miss�o iniciada: {currentMission.questName}");

            // Atualiza indicador visual
            UpdateVisualIndicator();

            // Inicia di�logo se dispon�vel
            if (dialogues.Count > currentMissionIndex && dialogues[currentMissionIndex] != null)
            {
                StartDialogue(currentMissionIndex);
            }
        }

        private void StartDialogue(int index)
        {
            if (index < 0 || index >= dialogues.Count || dialogues[index] == null)
                return;

            if (npcCamera != null)
            {
                CameraManager.Instance.dialogueCamera = npcCamera;
            }

            DialogueManager.Instance.StartDialogue(dialogues[index]);
        }

        #region Debug Methods
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void OnDrawGizmosSelected()
        {
            // Mostra onde o indicador ser� posicionado
            if (indicatorParent != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 indicatorPos = indicatorParent.position + indicatorOffset;
                Gizmos.DrawWireSphere(indicatorPos, 0.2f);
                Gizmos.DrawLine(indicatorParent.position, indicatorPos);
            }
        }

        [ContextMenu("Debug NPC State")]
        public void DebugNPCState()
        {
            Debug.Log($"=== NPC Debug ({npcID}) ===");
            Debug.Log($"GameObject Active: {gameObject.activeInHierarchy}");
            Debug.Log($"Component Enabled: {enabled}");
            Debug.Log($"Player Reference: {(player != null ? "OK" : "NULL")}");
            Debug.Log($"Missions Count: {(missions != null ? missions.Count : 0)}");
            Debug.Log($"Current Mission Index: {currentMissionIndex}");
            Debug.Log($"NPC State Exists: {(npcState != null)}");

            if (npcState != null)
            {
                Debug.Log($"NPC State - Has Started Current Mission: {npcState.hasStartedCurrentMission}");
                Debug.Log($"NPC State - Last Started Mission ID: {npcState.lastStartedMissionID}");
            }

            Debug.Log($"Has Available Mission: {HasAvailableMission()}");
            Debug.Log($"Simple Indicator: {(questAvailableIndicator != null ? $"EXISTS, Active: {questAvailableIndicator.activeSelf}" : "NULL")}");
            Debug.Log($"Visual Indicator Component: {(visualIndicatorComponent != null ? "EXISTS" : "NULL")}");
            Debug.Log($"QuestManager Instance: {(QuestManager.Instance != null ? "OK" : "NULL")}");
            Debug.Log($"NPCQuestStateManager Instance: {(NPCQuestStateManager.Instance != null ? "OK" : "NULL")}");

            if (currentMissionIndex < missions.Count && currentMissionIndex >= 0 && missions != null)
            {
                Quest current = missions[currentMissionIndex];
                if (current != null)
                {
                    Debug.Log($"Current Mission: {current.questName} (ID: {current.questID})");
                    Debug.Log($"Mission Completed: {QuestManager.Instance.IsMissionCompleted(current.questID)}");
                    Debug.Log($"Mission Started: {QuestManager.Instance.IsMissionStarted(current.questID)}");
                }
                else
                {
                    Debug.LogError($"Current mission is NULL at index {currentMissionIndex}!");
                }
            }
            else
            {
                Debug.Log($"No valid current mission - Index: {currentMissionIndex}, Missions: {(missions != null ? missions.Count.ToString() : "NULL")}");
            }

            // Testa BaseInteractable
            Debug.Log($"BaseInteractable Methods:");
            try
            {
                Debug.Log($"  Can Interact (should call base methods): Testing...");
                base.OnFocus();
                Debug.Log($"  base.OnFocus() - OK");
                base.OnLostFocus();
                Debug.Log($"  base.OnLostFocus() - OK");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"  Error calling base methods: {e.Message}");
            }

            Debug.Log($"================");
        }

        [ContextMenu("Force Update Indicator")]
        public void ForceUpdateIndicator()
        {
            Debug.Log($"[NPC {npcID}] Forcing indicator update...");
            UpdateVisualIndicator();
        }

        [ContextMenu("Test Interaction")]
        public void TestInteraction()
        {
            Debug.Log($"[NPC {npcID}] Testing interaction...");
            OnInteract();
        }
        #endregion

        #region Public Methods for External Control
        /// <summary>
        /// For�a a atualiza��o do estado do NPC (�til quando carregando save games)
        /// </summary>
        public void RefreshNPCState()
        {
            LoadNPCState(); // Recarrega estado do manager
            UpdateCurrentMissionIndex();

            // Reset do estado de miss�o iniciada se a miss�o atual mudou
            if (currentMissionIndex < missions.Count && currentMissionIndex >= 0)
            {
                string currentMissionID = missions[currentMissionIndex].questID;
                if (currentMissionID != npcState?.lastStartedMissionID)
                {
                    if (npcState != null)
                    {
                        npcState.hasStartedCurrentMission = false;
                        SaveNPCState();
                    }
                }
            }

            UpdateVisualIndicator();
        }

        /// <summary>
        /// Define manualmente se o NPC j� iniciou a miss�o atual (�til para save/load)
        /// </summary>
        public void SetMissionStartedState(bool started)
        {
            if (npcState != null)
            {
                npcState.hasStartedCurrentMission = started;
                if (started && currentMissionIndex < missions.Count && currentMissionIndex >= 0)
                {
                    npcState.lastStartedMissionID = missions[currentMissionIndex].questID;
                }
                SaveNPCState();
                UpdateVisualIndicator();
            }
        }

        /// <summary>
        /// Obt�m o ID �nico deste NPC
        /// </summary>
        public string GetNPCID()
        {
            return npcID;
        }

        /// <summary>
        /// Define o ID �nico deste NPC (use apenas se necess�rio)
        /// </summary>
        public void SetNPCID(string newID)
        {
            if (!string.IsNullOrEmpty(newID) && newID != npcID)
            {
                npcID = newID;
                LoadNPCState(); // Recarrega com novo ID
                UpdateVisualIndicator();
            }
        }
        #endregion
    }
}