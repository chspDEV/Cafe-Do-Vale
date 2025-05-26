using UnityEngine;
using UnityEngine.InputSystem;

namespace Tcp4
{
    public class PlayerInputs : MonoBehaviour
    {
        public void SetMap(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (UIManager.Instance != null && !UIManager.Instance.HasMenuOpen())
                {
                    UIManager.Instance.ControlMap(true);
                }
                else { Debug.LogWarning("InteractionManager não inicializado!"); }
            }
        }

        public void SetNotification(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (UIManager.Instance != null && !UIManager.Instance.HasMenuOpen())
                {
                    //UIManager.Instance.ControlNotification(true);
                }
                else { Debug.LogWarning("InteractionManager não inicializado!"); }
            }
        }

        public void SetSeedInventory(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (UIManager.Instance != null && !UIManager.Instance.HasMenuOpen())
                {
                    //UIManager.Instance.ControlSeedInventory(true);
                }
                else { Debug.LogWarning("InteractionManager não inicializado!"); }
            }
        }

        public void SetPause(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (UIManager.Instance != null && !UIManager.Instance.HasMenuOpen())
                {
                    UIManager.Instance.ControlConfigMenu();
                }
                else { Debug.LogWarning("InteractionManager não inicializado!"); }
            }
        }

        public void SetRecipe(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (UIManager.Instance != null && !UIManager.Instance.HasMenuOpen())
                {
                    UIManager.Instance.ControlRecipeMenu(true);
                }
                else { Debug.LogWarning("InteractionManager não inicializado!"); }
            }
        }

        public void SetCloseMenu(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.CloseLastMenu();
                }
                else { Debug.LogWarning("InteractionManager não inicializado!"); }
            }
        }
    }
}
