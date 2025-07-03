using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using Tcp4.Assets.Resources.Scripts.Managers;

namespace Tcp4
{
    public class NpcQuestInteractable : BaseInteractable
    {
        public List<Quest> missions; // Lista de ScriptableObjects de miss�o
        public bool showDebugMessages = true;
        public Animator anim;
        public string onLookAnimation;
        public string onInteractAnimation;
        public CinemachineCamera npcCamera;

        public List<DialogueData> dialogues;

        private int currentMissionIndex = -1;
        private GameObject player;
        private Quaternion idleRotation;

        public override void Start()
        {
            base.Start();
            UpdateCurrentMissionIndex();
            player = GameAssets.Instance.player;
            idleRotation = transform.rotation;
        }

        public override void OnFocus()
        {
            if (QuestManager.Instance.completedTutorials.Count <= 0) return;

            base.OnFocus();
            anim.Play(onLookAnimation);

            Vector3 direction = player.transform.position - transform.position;
            direction.y = 0; // Mant�m apenas a rota��o horizontal
            transform.rotation = Quaternion.LookRotation(direction * -1f) ;
        }

        public override void OnLostFocus()
        {
            if (QuestManager.Instance.completedTutorials.Count <= 0) return;

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

        public override void OnInteract()
        {
            if (QuestManager.Instance.completedTutorials.Count <= 0) return;

            base.OnInteract();

            // Todas as miss�es completas
            if (currentMissionIndex >= missions.Count)
            {
                if (showDebugMessages) Debug.Log("Todas as miss�es deste NPC foram completadas!");
                return;
            }

            Quest currentMission = missions[currentMissionIndex];
            bool missionCompleted = QuestManager.Instance.IsMissionCompleted(currentMission.questID);
            bool missionStarted = QuestManager.Instance.IsMissionStarted(currentMission.questID);

            if (missionCompleted)
            {
                // Avan�a para pr�xima miss�o
                currentMissionIndex++;
                UpdateCurrentMissionIndex();

                if (currentMissionIndex < missions.Count)
                {
                    StartCurrentMission();
                }
            }
            else if (!missionStarted)
            {
                StartCurrentMission();
            }
            else
            {
                if (showDebugMessages) Debug.Log($"Miss�o em progresso: {currentMission.questName}");

                // Reinicia di�logo se necess�rio
                if (dialogues.Count > currentMissionIndex && dialogues[currentMissionIndex] != null)
                {
                    StartDialogue(currentMissionIndex);
                }
            }
        }

        private void StartCurrentMission()
        {
            Quest currentMission = missions[currentMissionIndex];

            // Registra no QuestManager como iniciada
            QuestManager.Instance.StartMission(currentMission.questID);

            if (showDebugMessages) Debug.Log($"Miss�o iniciada: {currentMission.questName}");

            // Inicia di�logo se dispon�vel
            if (dialogues.Count > currentMissionIndex && dialogues[currentMissionIndex] != null)
            {
                StartDialogue(currentMissionIndex);
            }
        }

        private void StartDialogue(int index)
        {
            if (npcCamera != null)
            {
                // Descomente quando implementar
                // CameraManager.Instance.SetupDialogueCamera(npcCamera);
            }

            // Descomente quando implementar
            // DialogueManager.Instance.StartDialogue(dialogues[index]);
        }
    }
}