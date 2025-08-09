using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

namespace Tcp4
{
    public class CameraManager : Singleton<CameraManager>
    {
        [Header("Refer�ncias de C�mera")]
        [SerializeField] private CinemachineCamera playerCamera;
        [SerializeField] public CinemachineCamera dialogueCamera;

        // N�o precisamos mais da refer�ncia 'unlockCamera' aqui

        private Transform playerTarget; // O alvo que a playerCamera deve seguir
        private Coroutine temporaryLookCoroutine;

        private void Start()
        {
            // Garante que a c�mera de di�logo comece desativada e a do jogador ativa
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
            // Desativa a c�mera de di�logo antiga, se houver
            if (dialogueCamera != null && dialogueCamera != newCam)
            {
                dialogueCamera.gameObject.SetActive(false);
            }

            dialogueCamera = newCam;

            if (dialogueCamera != null)
            {
                dialogueCamera.gameObject.SetActive(active);
            }

            // A c�mera do jogador sempre faz o oposto da c�mera de di�logo
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
            // VERIFICA��O DE NULIDADE: Garante que a c�mera existe antes de continuar.
            if (temporaryCam == null)
            {
                Debug.LogError("ShowTemporaryCameraRoutine foi chamada com uma c�mera nula! Interrompendo.");
                yield break; // Interrompe a coroutine de forma segura.
            }

            Debug.Log($"[CameraManager] Mostrando c�mera tempor�ria: {temporaryCam.name} por {duration}s. Prioridade: {temporaryCam.Priority}");

            // Ativa a c�mera tempor�ria, o Cinemachine far� a transi��o.
            temporaryCam.gameObject.SetActive(true);

            // LOG: Antes de esperar, vamos checar se a playerCamera est� ativa.
            if (playerCamera != null)
            {
                Debug.Log($"[CameraManager] No in�cio, a playerCamera '{playerCamera.name}' est� ativa? {playerCamera.gameObject.activeInHierarchy}");
            }

            yield return new WaitForSeconds(duration);

            Debug.Log($"[CameraManager] Tempo esgotado. Retornando para a c�mera principal.");

            // LOG: Antes de desativar, vamos checar o estado da playerCamera novamente.
            if (playerCamera != null)
            {
                Debug.Log($"[CameraManager] No final, a playerCamera '{playerCamera.name}' est� ativa? {playerCamera.gameObject.activeInHierarchy}");
            }

            // Desativa a c�mera tempor�ria, o Cinemachine deve voltar para a c�mera anterior.
            temporaryCam.gameObject.SetActive(false);

            Debug.Log($"[CameraManager] C�mera tempor�ria '{temporaryCam.name}' desativada. Transi��o conclu�da.");
        }

        // Forma correta de mover a c�mera do jogador para um alvo tempor�rio
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
            // 1. Diz para a c�mera do jogador olhar para o novo alvo
            if (playerCamera != null)
            {
                playerCamera.LookAt = temporaryTarget;
            }

            // 2. Espera a dura��o
            yield return new WaitForSeconds(duration);

            // 3. Diz para a c�mera do jogador voltar a olhar para seu alvo original
            if (playerCamera != null)
            {
                playerCamera.LookAt = playerTarget;
            }

            temporaryLookCoroutine = null;
        }
    }
}