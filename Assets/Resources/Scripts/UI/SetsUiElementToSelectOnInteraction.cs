using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using Sirenix.OdinInspector;

namespace ChristinaCreatesGames.UI
{
    public class SetsUiElementToSelectOnInteraction : MonoBehaviour
    {
        [FoldoutGroup("Setup")]
        [Required]
        [SerializeField]
        private EventSystem eventSystem;

        [FoldoutGroup("Setup")]
        [SerializeField]
        private Selectable elementToSelect;

        [FoldoutGroup("Tag Search")]
        [LabelText("Search By Tag")]
        [SerializeField]
        private bool searchByTag;

        [FoldoutGroup("Tag Search")]
        [ShowIf("searchByTag")]
        [LabelText("Tag To Search")]
        [SerializeField]
        private string tagToSearch;

        [FoldoutGroup("Visualization")]
        [LabelText("Show Gizmo")]
        [SerializeField]
        private bool showVisualization;

        [FoldoutGroup("Visualization")]
        [ShowIf("showVisualization")]
        [LabelText("Navigation Color")]
        [SerializeField]
        private Color navigationColour = Color.cyan;

        private void Start()
        {
            StartCoroutine(InitializeRoutine());
        }

        private IEnumerator InitializeRoutine()
        {
            // Ensure EventSystem is assigned
            if (eventSystem == null)
                eventSystem = FindFirstObjectByType<EventSystem>();

            // If tag search is enabled, find first matching GameObject over frames
            if (searchByTag && !string.IsNullOrEmpty(tagToSearch))
            {
                GameObject found = null;
                while (found == null)
                {
                    found = GameObject.FindWithTag(tagToSearch);
                    yield return null; // wait one frame to avoid blocking
                }

                // Try get Selectable on object or its children
                elementToSelect = found.GetComponent<Selectable>()
                                 ?? found.GetComponentInChildren<Selectable>(true);
            }



            yield break;
        }

        private void OnDrawGizmos()
        {
            if (!showVisualization || elementToSelect == null)
                return;

            Gizmos.color = navigationColour;
            Gizmos.DrawLine(transform.position, elementToSelect.transform.position);
        }

        /// <summary>
        /// Jumps the EventSystem selection to the configured UI element.
        /// </summary>
        public void JumpToElement()
        {
            if (eventSystem == null || elementToSelect == null) return;

            eventSystem.SetSelectedGameObject(elementToSelect.gameObject);

        }
    }
}
