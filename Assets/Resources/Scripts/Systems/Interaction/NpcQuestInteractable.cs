using UnityEngine;
using System.Linq;
using System;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.Cinemachine;
namespace Tcp4
{
    public enum NpcQuestStatus
    { 
        Completed,
        Started,
        Not_Started
    }

    [Serializable]
    public class NpcQuest
    {
        public string mission_id;
        public NpcQuestStatus missionState;
    }

    public class NpcQuestInteractable : BaseInteractable
    {
        public NpcQuest[] missions; 
        public bool showDebugMessages = true;
        public Animator anim;
        public string onLookAnimation;
        public string onInteractAnimation;
        public CinemachineCamera npcCamera;

        public List<DialogueData> dialogues;

        private int currentMissionIndex = -1;

        public override void Start()
        {
            base.Start();
            // Encontra o ponto de progresso atual
            UpdateCurrentMissionIndex();
        }

        public override void OnFocus()
        {
            base.OnFocus();
            anim.Play(onLookAnimation);
        }

        private void UpdateCurrentMissionIndex()
        {
            for (int i = 0; i < missions.Length; i++)
            {
                bool missionCompleted = QuestManager.Instance.IsMissionCompleted(missions[i].mission_id);
                bool missionStarted = IsMissionStarted(missions[i]);

                if (!missionCompleted || (missionStarted && !missionCompleted))
                {
                    currentMissionIndex = i;
                    return;
                }
            }

            // Se todas as miss�es estiverem completas
            currentMissionIndex = missions.Length;
        }

        public override void OnInteract()
        {
            base.OnInteract();

            // Todas as miss�es completas
            if (currentMissionIndex >= missions.Length)
            {
                if (showDebugMessages) Debug.Log("Todas as miss�es deste NPC foram completadas!");
                return;
            }

            NpcQuest currentMission = missions[currentMissionIndex];

            // Se a miss�o atual foi completada, avan�a para a pr�xima
            if (QuestManager.Instance.IsMissionCompleted(currentMission.mission_id))
            {
                currentMissionIndex++;
                UpdateCurrentMissionIndex();

                if (currentMissionIndex < missions.Length)
                {
                    StartCurrentMission();
                }
                return;
            }

            // Se a miss�o atual n�o foi iniciada
            if (!IsMissionStarted(currentMission))
            {
                StartCurrentMission();
                return;
            }

            // Se chegou aqui, a miss�o est� em andamento
            if (showDebugMessages) Debug.Log($"Miss�o em progresso: {currentMission}");
        }

        private bool IsMissionStarted(NpcQuest mission)
        {
            return mission.missionState == NpcQuestStatus.Started;
        }

        private void StartCurrentMission()
        {
            string mission = missions[currentMissionIndex].mission_id;
            QuestManager.Instance.StartMission(mission);

            if (dialogues[currentMissionIndex] != null)
            {
                if (npcCamera != null)
                {
                    CameraManager.Instance.SetupDialogueCamera(npcCamera);
                }

                DialogueManager.Instance.StartDialogue(dialogues[currentMissionIndex]);
            }

            if (showDebugMessages) Debug.Log($"Miss�o iniciada: {mission}");
        }
    }
}
