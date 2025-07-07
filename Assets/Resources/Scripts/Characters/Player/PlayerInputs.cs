using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tcp4
{
    public class PlayerInputs : MonoBehaviour
    {
        public bool map, notificiation, seedInventory, pause, recipe;

        public void SetMap(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (UIManager.Instance != null && !UIManager.Instance.HasMenuOpen())
                {
                    UIManager.Instance.ControlMap(true);
                    map = true;
                    StartCoroutine(ResetCheckInputs());
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
                    notificiation = true;
                    StartCoroutine(ResetCheckInputs());
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
                    seedInventory = true;
                    StartCoroutine(ResetCheckInputs());
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
                    pause = true;
                    StartCoroutine(ResetCheckInputs());
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
                    recipe = true;
                    StartCoroutine(ResetCheckInputs());
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

        IEnumerator ResetCheckInputs()
        {
            yield return new WaitForSeconds(1.3f);
            
            map = false;
            notificiation = false;
            seedInventory = false;
            pause = false; 
            recipe = false;
        }
    }
}
