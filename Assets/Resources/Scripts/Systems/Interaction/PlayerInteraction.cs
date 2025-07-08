using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tcp4
{
    public class PlayerInteraction : MonoBehaviour
    {
        public bool interactionPressed;

        public void SetInteraction(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (InteractionManager.Instance != null)
                {

                    InteractionManager.Instance.TryInteract();
                    interactionPressed = true;
                    StartCoroutine(ResetInteraction());

                }
                else { Debug.LogWarning("InteractionManager não inicializado!"); }
            }
        }

        private IEnumerator ResetInteraction()
        {
            yield return new WaitForSeconds(0.2f);
            interactionPressed = false;
        }

        private void OnValidate()
        {
            if (!gameObject.CompareTag("Player"))
            {
                Debug.LogWarning("Este objeto deve ter a tag 'Player' para o InteractionManager funcionar!");
                gameObject.tag = "Player";
            }
        }
    }
}