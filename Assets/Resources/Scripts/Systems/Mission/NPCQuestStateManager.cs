using UnityEngine;
using System.Collections.Generic;

namespace Tcp4
{
    
    /// <summary>
    /// Manager para controlar estados de todos os NPCs com quest
    /// </summary>
    public class NPCQuestStateManager : Singleton<NPCQuestStateManager>
    {

        [SerializeField] private Dictionary<string, NPCQuestState> npcStates = new Dictionary<string, NPCQuestState>();

        // Para serialização no Inspector (Unity não serializa Dictionary diretamente)
        [SerializeField] private List<NPCQuestState> npcStatesList = new List<NPCQuestState>();

        public override void Awake()
        {
            base.Awake();
            LoadStatesFromList();
            
        }

        private void LoadStatesFromList()
        {
            npcStates.Clear();
            foreach (var state in npcStatesList)
            {
                if (!string.IsNullOrEmpty(state.npcID))
                    npcStates[state.npcID] = state;
            }
        }

        private void SaveStatesToList()
        {
            npcStatesList.Clear();
            foreach (var kvp in npcStates)
            {
                npcStatesList.Add(kvp.Value);
            }
        }

        /// <summary>
        /// Obtém o estado de um NPC, criando um novo se não existir
        /// </summary>
        public NPCQuestState GetNPCState(string npcID)
        {
            if (string.IsNullOrEmpty(npcID))
            {
                Debug.LogError("NPCQuestStateManager: npcID cannot be null or empty!");
                return null;
            }

            if (!npcStates.ContainsKey(npcID))
            {
                npcStates[npcID] = new NPCQuestState(npcID);
                SaveStatesToList();
            }

            return npcStates[npcID];
        }

        /// <summary>
        /// Atualiza o estado de um NPC
        /// </summary>
        public void UpdateNPCState(string npcID, NPCQuestState newState)
        {
            if (string.IsNullOrEmpty(npcID) || newState == null) return;

            npcStates[npcID] = newState;
            SaveStatesToList();
        }

        /// <summary>
        /// Marca que um NPC iniciou uma missão
        /// </summary>
        public void SetNPCMissionStarted(string npcID, string missionID, int missionIndex)
        {
            var state = GetNPCState(npcID);
            if (state != null)
            {
                state.hasStartedCurrentMission = true;
                state.lastStartedMissionID = missionID;
                state.currentMissionIndex = missionIndex;
                SaveStatesToList();
            }
        }

        /// <summary>
        /// Adiciona um diálogo como completado para um NPC
        /// </summary>
        public void AddCompletedDialogue(string npcID, string dialogueID)
        {
            var state = GetNPCState(npcID);
            if (state != null && !state.completedDialogues.Contains(dialogueID))
            {
                state.completedDialogues.Add(dialogueID);
                SaveStatesToList();
            }
        }

        /// <summary>
        /// Verifica se um NPC já completou um diálogo específico
        /// </summary>
        public bool HasCompletedDialogue(string npcID, string dialogueID)
        {
            var state = GetNPCState(npcID);
            return state != null && state.completedDialogues.Contains(dialogueID);
        }

        /// <summary>
        /// Reset completo do estado de um NPC (útil para debug/testing)
        /// </summary>
        public void ResetNPCState(string npcID)
        {
            if (npcStates.ContainsKey(npcID))
            {
                npcStates[npcID] = new NPCQuestState(npcID);
                SaveStatesToList();
            }
        }

        /// <summary>
        /// Reset de todos os estados (útil para debug/testing)
        /// </summary>
        [ContextMenu("Reset All NPC States")]
        public void ResetAllNPCStates()
        {
            npcStates.Clear();
            npcStatesList.Clear();
            Debug.Log("All NPC states have been reset!");
        }

        #region Save/Load Integration
        /// <summary>
        /// Obtém dados para salvar no sistema de save
        /// </summary>
        public string GetSaveData()
        {
            SaveStatesToList();
            return JsonUtility.ToJson(new SerializableNPCStateData { states = npcStatesList });
        }

        /// <summary>
        /// Carrega dados do sistema de save
        /// </summary>
        public void LoadSaveData(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData)) return;

            try
            {
                var data = JsonUtility.FromJson<SerializableNPCStateData>(jsonData);
                if (data != null && data.states != null)
                {
                    npcStatesList = data.states;
                    LoadStatesFromList();
                    Debug.Log($"Loaded {npcStates.Count} NPC states from save data");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading NPC state data: {e.Message}");
            }
        }

        [System.Serializable]
        private class SerializableNPCStateData
        {
            public List<NPCQuestState> states;
        }
        #endregion

        #region Debug Methods
        [ContextMenu("Debug All States")]
        public void DebugAllStates()
        {
            Debug.Log("=== NPC Quest States Debug ===");
            foreach (var kvp in npcStates)
            {
                var state = kvp.Value;
                Debug.Log($"NPC: {state.npcID}");
                Debug.Log($"  Current Mission Index: {state.currentMissionIndex}");
                Debug.Log($"  Has Started Current: {state.hasStartedCurrentMission}");
                Debug.Log($"  Last Started Mission: {state.lastStartedMissionID}");
                Debug.Log($"  Completed Dialogues: {string.Join(", ", state.completedDialogues)}");
                Debug.Log("---");
            }
            Debug.Log("==============================");
        }
        #endregion
    }
}