using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using Sirenix.OdinInspector;

namespace Tcp4
{
    public class UI_Book : MonoBehaviour
    {
        [TabGroup("Tab Buttons")] public Button[] tabButtons;
        [TabGroup("Tab Buttons")] [SerializeField] private Color highlightColor = Color.white;

        [Header("Controle")]
        [TabGroup("Tab Buttons")] public float inputCooldown = 0.3f;

        [TabGroup("Sections")] public GameObject[] sections;

        private int currentTab = 0;
        private bool canChangeTab = true;
        private EventSystem eventSystem;

        void Start()
        {
            if (eventSystem == null)
                eventSystem = EventSystem.current != null ? EventSystem.current : FindFirstObjectByType<EventSystem>();

            eventSystem = EventSystem.current;
            DisableTabNavigation();
            ShowTab(currentTab);
        }

        public void OnNextTab(InputAction.CallbackContext context)
        {
            if (!gameObject.activeInHierarchy) return;
            if (context.performed) ChangeTab(1);
        }

        public void OnPreviousTab(InputAction.CallbackContext context)
        {
            if (!gameObject.activeInHierarchy) return;
            if (context.performed) ChangeTab(-1);
        }

        void ChangeTab(int direction)
        {
            if (!canChangeTab) return;
            StartCoroutine(TabCooldown());

            currentTab = (currentTab + direction + tabButtons.Length) % tabButtons.Length;
            ShowTab(currentTab);
        }

        void ShowTab(int index)
        {
            for (int i = 0; i < sections.Length; i++)
                sections[i].SetActive(i == index);

            HighlightTabButton(index);
            SelectFirstElementInSection(index);
        }

        void SelectFirstElementInSection(int index)
        {

            if (sections == null || index < 0 || index >= sections.Length || sections[index] == null || eventSystem == null)
                return;

            var buttonsInSection = sections[index].GetComponentsInChildren<Selectable>();
            if (buttonsInSection == null || buttonsInSection.Length == 0 || buttonsInSection[0] == null)
                return;

            eventSystem.SetSelectedGameObject(buttonsInSection[0].gameObject);
        }

        void HighlightTabButton(int index)
        {
            foreach (var btn in tabButtons)
            {
                var colors = btn.colors;
                colors.normalColor = (btn == tabButtons[index]) ? highlightColor : Color.white;
                btn.colors = colors;
            }
        }

        void DisableTabNavigation()
        {
            foreach (var btn in tabButtons)
                btn.navigation = new Navigation { mode = Navigation.Mode.None };
        }

        IEnumerator TabCooldown()
        {
            canChangeTab = false;
            yield return new WaitForSeconds(inputCooldown);
            canChangeTab = true;
        }

        public void ForceShowTab(int index)
        {
            currentTab = index;
            ShowTab(index);
        }

    }
}
