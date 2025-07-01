using UnityEngine;

namespace Tcp4
{
    using UnityEngine;
    using System.Collections;
    using Unity.Cinemachine;

    public class CameraManager : Singleton<CameraManager>
    {
        [Header("Referências de Câmera")]
        [SerializeField] private CinemachineCamera playerCamera;
        [SerializeField] private CinemachineCamera dialogueCamera;

        [Header("Configurações da Player Camera")]
        [SerializeField] private float returnSpeed = 5f;

        private Transform playerTarget;
        private bool isReturningToPlayer = false;
        private Coroutine moveRoutine;

        private void Start()
        {
            // Garante que a câmera de diálogo comece desativada
            SetDialogueCameraActive(false);
        }

        private void LateUpdate()
        {
            // Retorno suave ao jogador após uma interrupção
            if (isReturningToPlayer && playerTarget != null)
            {
                playerCamera.transform.position = Vector3.Lerp(
                    playerCamera.transform.position,
                    playerTarget.position,
                    returnSpeed * Time.deltaTime
                );
            }
        }

        /// <summary>
        /// Ativa/desativa a câmera de diálogo
        /// </summary>
        public void SetDialogueCameraActive(bool active)
        {
            if (dialogueCamera != null)
            {
                dialogueCamera.gameObject.SetActive(active);
                playerCamera.gameObject.SetActive(!active);
            }
        }

        public void SetupDialogueCamera(CinemachineCamera newCam)
        {
            if (newCam != null)
            {
                dialogueCamera = newCam;
            }
        }

        /// <summary>
        /// Define o alvo de acompanhamento padrão da câmera do jogador
        /// </summary>
        public void SetPlayerTarget(Transform target)
        {
            playerTarget = target;
        }

        /// <summary>
        /// Move a câmera do jogador para uma posição específica temporariamente
        /// </summary>
        /// <param name="targetPosition">Posição de destino</param>
        /// <param name="duration">Tempo de permanência na posição (segundos)</param>
        public void MovePlayerCameraTemporarily(Vector3 targetPosition, float duration)
        {
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
            }
            moveRoutine = StartCoroutine(MoveCameraRoutine(targetPosition, duration));
        }

        private IEnumerator MoveCameraRoutine(Vector3 targetPosition, float duration)
        {
            // 1. Guarda o estado original
            Vector3 originalPosition = playerCamera.transform.position;

            // 2. Desativa o acompanhamento do jogador
            isReturningToPlayer = false;

            // 3. Move instantaneamente para a nova posição
            playerCamera.transform.position = targetPosition;

            // 4. Mantém na posição pelo tempo especificado
            yield return new WaitForSeconds(duration);

            // 5. Retorna para o jogador
            isReturningToPlayer = true;

            // 6. Restaura o estado após chegar perto do alvo
            yield return new WaitUntil(() =>
                Vector3.Distance(playerCamera.transform.position, playerTarget.position) < 0.1f);

            isReturningToPlayer = false;
            moveRoutine = null;
        }
    }
}
