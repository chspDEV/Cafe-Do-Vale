using System.Collections;
using UnityEngine;
using PlugInputPack;
using Tcp4.Assets.Resources.Scripts.Managers;

namespace Tcp4
{
    public class PlayerInputs : MonoBehaviour
    {
        [Header("Input Plug Asset")]
        public PlugInputComponent input;

        [Header("Debug Flags")]
        public bool map, notification, seedInventory, pause, recipe;

        public bool CanCloseMenu = true;

        private bool canCheckInputs = true;
        private void Start()
        {
            input = GameAssets.Instance.inputComponent;
        }

        IEnumerator WaitCloseMenu()
        {
            CanCloseMenu = false;
            yield return new WaitForSeconds(0.1f);
            CanCloseMenu = true;
        }
        private void Update()
        {
            if (input == null || UIManager.Instance == null) return;

            if (input["Pause"].Pressed && !UIManager.Instance.HasMenuOpen())
            {
                UIManager.Instance.ControlConfigMenu(true);
                pause = true;
                StartCoroutine(ResetCheckInputs());
                StartCoroutine(WaitCloseMenu());
            }

            // Fecha menu sempre que possível
            if (input["CloseMenu"].Pressed && UIManager.Instance.HasMenuOpen() && CanCloseMenu)
            {
                UIManager.Instance.CloseLastMenu();
                return; // evita conflito com o resto dos inputs
            }

            // Impede inputs de abertura se estiver bloqueado
            if (!canCheckInputs) return;

            if (input["Map"].Pressed && !UIManager.Instance.HasMenuOpen())
            {
                UIManager.Instance.ControlMap(true);
                map = true;
                StartCoroutine(ResetCheckInputs());
            }

            if (input["Notification"].Pressed && !UIManager.Instance.HasMenuOpen())
            {
                // UIManager.Instance.ControlNotification(true);
                notification = true;
                StartCoroutine(ResetCheckInputs());
            }

            if (input["SeedInventory"].Pressed && !UIManager.Instance.HasMenuOpen())
            {
                UIManager.Instance.ControlSeedInventory(true);
                seedInventory = true;
                StartCoroutine(ResetCheckInputs());
            }

            

            if (input["Recipe"].Pressed && !UIManager.Instance.HasMenuOpen())
            {
                UIManager.Instance.ControlRecipeMenu(true);
                recipe = true;
                StartCoroutine(ResetCheckInputs());
            }
        }


        private IEnumerator ResetCheckInputs()
        {
            canCheckInputs = false;
            yield return new WaitForSeconds(0.2f);
            map = false;
            notification = false;
            seedInventory = false;
            pause = false;
            recipe = false;
            canCheckInputs = true;
        }
    }
}
