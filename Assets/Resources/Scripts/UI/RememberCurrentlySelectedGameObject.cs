using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RememberCurrentlySelectedGameObject : MonoBehaviour
{
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private GameObject lastSelectedElement;
    [Header("Nunca salvar como sele��o")]
    [SerializeField] private GameObject buttonToIgnore; // Referencie o bot�o Criar aqui

    private void Reset()
    {
        eventSystem = FindFirstObjectByType<EventSystem>();

        if (!eventSystem)
        {
            Debug.Log("Did not find an Event System in this scene.", this);
            return;
        }

        lastSelectedElement = eventSystem.firstSelectedGameObject;
    }

    private void Update()
    {
        if (!eventSystem)
            return;

        var current = eventSystem.currentSelectedGameObject;

        // S� salva se n�o for o bot�o a ser ignorado
        if (current && lastSelectedElement != current && current != buttonToIgnore)
            lastSelectedElement = current;

        if (!eventSystem.currentSelectedGameObject && lastSelectedElement)
            eventSystem.SetSelectedGameObject(lastSelectedElement);
    }
}






public class EventSystemAccess : MonoBehaviour
{
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private Selectable firstItemToSelect;

    private void Start()
    {
        if (eventSystem == null)
            return;

        eventSystem.firstSelectedGameObject = firstItemToSelect.gameObject;
    }
}