using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using Sirenix.OdinInspector;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine.SceneManagement;

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
            if (eventSystem == null)
                eventSystem = FindFirstObjectByType<EventSystem>();

            if (searchByTag && !string.IsNullOrEmpty(tagToSearch))
            {
                GameObject found = null;

                // Tenta encontrar o objeto com a tag
                while (found == null)
                {
                    found = GameObject.FindWithTag(tagToSearch);
                    yield return null;
                }

                // Espera até que o objeto esteja ativo
                while (!found.activeInHierarchy)
                    yield return null;

                // Tenta obter o componente Selectable (ativo ou não)
                elementToSelect = found.GetComponent<Selectable>()
                    ?? found.GetComponentInChildren<Selectable>(true);

                // Espera até o Selectable existir e estar ativo na hierarquia
                while (elementToSelect == null || !elementToSelect.gameObject.activeInHierarchy)
                    yield return null;

                eventSystem.SetSelectedGameObject(elementToSelect.gameObject);
            }

            yield break;
        }

        /// <summary>
        /// Seleciona o elemento diretamente.
        /// Use isso se você já sabe qual prefab foi instanciado.
        /// </summary>
        public void SetSelectable(GameObject instance)
        {
            if (eventSystem == null)
                eventSystem = FindFirstObjectByType<EventSystem>();

            elementToSelect = instance.GetComponent<Selectable>()
                ?? instance.GetComponentInChildren<Selectable>(true);

            if (elementToSelect != null && elementToSelect.gameObject.activeInHierarchy)
            {
                eventSystem.SetSelectedGameObject(elementToSelect.gameObject);
            }
            else
            {
                Debug.LogWarning($"{nameof(SetsUiElementToSelectOnInteraction)}: Elemento instanciado não possui um Selectable ativo.");
            }
        }

        /// <summary>
        /// Pode ser chamado manualmente, por exemplo ao abrir um menu.
        /// </summary>
        public void JumpToElement()
        {
            if (eventSystem == null)
            {
                Debug.LogWarning($"{nameof(SetsUiElementToSelectOnInteraction)}: EventSystem não está definido.");
                return;
            }

            if (elementToSelect == null || !elementToSelect.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"{nameof(SetsUiElementToSelectOnInteraction)}: Nenhum elemento selecionável está ativo.");
                return;
            }

            eventSystem.SetSelectedGameObject(elementToSelect.gameObject);
        }

        private void OnDrawGizmos()
        {
            if (!showVisualization || elementToSelect == null)
                return;

            Gizmos.color = navigationColour;
            Gizmos.DrawLine(transform.position, elementToSelect.transform.position);
        }
    }
}
