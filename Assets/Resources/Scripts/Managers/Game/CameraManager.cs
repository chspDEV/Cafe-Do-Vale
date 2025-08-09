using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

namespace Tcp4
{
    public class CameraManager : Singleton<CameraManager>
    {
        [Header("Referências de Câmera")]
        [SerializeField] private CinemachineCamera playerCamera;
        [SerializeField] public CinemachineCamera dialogueCamera;

        // Não precisamos mais da referência 'unlockCamera' aqui

        private Transform playerTarget; // O alvo que a playerCamera deve seguir
        private Coroutine temporaryLookCoroutine;

        private void Start()
        {
            // Garante que a câmera de diálogo comece desativada e a do jogador ativa
            if (dialogueCamera != null) dialogueCamera.gameObject.SetActive(false);
            if (playerCamera != null) playerCamera.gameObject.SetActive(true);
        }

        public void SetPlayerTarget(Transform target)
        {
            playerTarget = target;
            if (playerCamera != null)
            {
                playerCamera.Follow = playerTarget;
                playerCamera.LookAt = playerTarget;
            }
        }

        public void SetDialogueCamera(CinemachineCamera newCam, bool active)
        {
            // Desativa a câmera de diálogo antiga, se houver
            if (dialogueCamera != null && dialogueCamera != newCam)
            {
                dialogueCamera.gameObject.SetActive(false);
            }

            dialogueCamera = newCam;

            if (dialogueCamera != null)
            {
                dialogueCamera.gameObject.SetActive(active);
            }

            // A câmera do jogador sempre faz o oposto da câmera de diálogo
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(!active);
            }
        }

        public void ShowTemporaryCamera(CinemachineCamera temporaryCam, float duration)
        {
            StartCoroutine(ShowTemporaryCameraRoutine(temporaryCam, duration));
        }

        // Dentro do seu script CameraManager.cs

        private IEnumerator ShowTemporaryCameraRoutine(CinemachineCamera temporaryCam, float duration)
        {
            // VERIFICAÇÃO DE NULIDADE: Garante que a câmera existe antes de continuar.
            if (temporaryCam == null)
            {
                Debug.LogError("ShowTemporaryCameraRoutine foi chamada com uma câmera nula! Interrompendo.");
                yield break; // Interrompe a coroutine de forma segura.
            }

            Debug.Log($"[CameraManager] Mostrando câmera temporária: {temporaryCam.name} por {duration}s. Prioridade: {temporaryCam.Priority}");

            // Ativa a câmera temporária, o Cinemachine fará a transição.
            temporaryCam.gameObject.SetActive(true);

            // LOG: Antes de esperar, vamos checar se a playerCamera está ativa.
            if (playerCamera != null)
            {
                Debug.Log($"[CameraManager] No início, a playerCamera '{playerCamera.name}' está ativa? {playerCamera.gameObject.activeInHierarchy}");
            }

            yield return new WaitForSeconds(duration);

            Debug.Log($"[CameraManager] Tempo esgotado. Retornando para a câmera principal.");

            // LOG: Antes de desativar, vamos checar o estado da playerCamera novamente.
            if (playerCamera != null)
            {
                Debug.Log($"[CameraManager] No final, a playerCamera '{playerCamera.name}' está ativa? {playerCamera.gameObject.activeInHierarchy}");
            }

            // Desativa a câmera temporária, o Cinemachine deve voltar para a câmera anterior.
            temporaryCam.gameObject.SetActive(false);

            Debug.Log($"[CameraManager] Câmera temporária '{temporaryCam.name}' desativada. Transição concluída.");
        }

        // Forma correta de mover a câmera do jogador para um alvo temporário
        public void LookAtTargetTemporarily(Transform temporaryTarget, float duration)
        {
            if (temporaryLookCoroutine != null)
            {
                StopCoroutine(temporaryLookCoroutine);
            }
            temporaryLookCoroutine = StartCoroutine(LookAtTargetRoutine(temporaryTarget, duration));
        }

        private IEnumerator LookAtTargetRoutine(Transform temporaryTarget, float duration)
        {
            // 1. Diz para a câmera do jogador olhar para o novo alvo
            if (playerCamera != null)
            {
                playerCamera.LookAt = temporaryTarget;
            }

            // 2. Espera a duração
            yield return new WaitForSeconds(duration);

            // 3. Diz para a câmera do jogador voltar a olhar para seu alvo original
            if (playerCamera != null)
            {
                playerCamera.LookAt = playerTarget;
            }

            temporaryLookCoroutine = null;
        }
    }
}