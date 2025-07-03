using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tcp4
{
    public class PlayerInteraction : MonoBehaviour
    {
        public event Action OnPlayerInteraction;

        public void SetInteraction(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (InteractionManager.Instance != null)
                {

                    InteractionManager.Instance.TryInteract();
                    OnPlayerInteraction?.Invoke();

                }
                else { Debug.LogWarning("InteractionManager não inicializado!"); }
            }
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