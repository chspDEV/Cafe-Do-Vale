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
                else { Debug.LogWarning("UIManager não inicializado ou outro menu aberto!"); }
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
                else { Debug.LogWarning("UIManager não inicializado ou outro menu aberto!"); }
            }
        }

        public void SetSeedInventory(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (UIManager.Instance != null && !UIManager.Instance.HasMenuOpen())
                {
                    UIManager.Instance.ControlSeedInventory(true);
                }
                else { Debug.LogWarning("UIManager não inicializado ou outro menu aberto."); }
            }
        }

        public void SetPause(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (UIManager.Instance != null && !UIManager.Instance.HasMenuOpen())
                {
                    UIManager.Instance.ControlConfigMenu(true);
                }
                else { Debug.LogWarning("UIManager não inicializado ou outro menu aberto!"); }
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
                else { Debug.LogWarning("UIManager não inicializado ou outro menu aberto!"); }
            }
        }

        public void SetCloseMenu(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (UIManager.Instance != null && UIManager.Instance.HasMenuOpen())
                {
                    UIManager.Instance.CloseLastMenu();
                }
                else { Debug.LogWarning("UIManager não inicializado ou outro menu aberto!"); }
            }
        }
    }
}
