using UnityEngine;

namespace Tcp4
{
    using UnityEngine;
    using System.Collections;
    using Unity.Cinemachine;

    public class CameraManager : Singleton<CameraManager>
    {
        [Header("Refer�ncias de C�mera")]
        [SerializeField] private CinemachineCamera playerCamera;
        [SerializeField] private CinemachineCamera dialogueCamera;

        [Header("Configura��es da Player Camera")]
        [SerializeField] private float returnSpeed = 5f;

        private Transform playerTarget;
        private bool isReturningToPlayer = false;
        private Coroutine moveRoutine;

        private void Start()
        {
            // Garante que a c�mera de di�logo comece desativada
            SetDialogueCameraActive(false);
        }

        private void LateUpdate()
        {
            // Retorno suave ao jogador ap�s uma interrup��o
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
        /// Ativa/desativa a c�mera de di�logo
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
        /// Define o alvo de acompanhamento padr�o da c�mera do jogador
        /// </summary>
        public void SetPlayerTarget(Transform target)
        {
            playerTarget = target;
        }

        /// <summary>
        /// Move a c�mera do jogador para uma posi��o espec�fica temporariamente
        /// </summary>
        /// <param name="targetPosition">Posi��o de destino</param>
        /// <param name="duration">Tempo de perman�ncia na posi��o (segundos)</param>
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

            // 3. Move instantaneamente para a nova posi��o
            playerCamera.transform.position = targetPosition;

            // 4. Mant�m na posi��o pelo tempo especificado
            yield return new WaitForSeconds(duration);

            // 5. Retorna para o jogador
            isReturningToPlayer = true;

            // 6. Restaura o estado ap�s chegar perto do alvo
            yield return new WaitUntil(() =>
                Vector3.Distance(playerCamera.transform.position, playerTarget.position) < 0.1f);

            isReturningToPlayer = false;
            moveRoutine = null;
        }
    }
}
