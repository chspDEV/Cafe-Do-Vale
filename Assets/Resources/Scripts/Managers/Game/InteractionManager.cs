using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class InteractionManager : Singleton<InteractionManager>
{
    [Header("Configurações")]
    [SerializeField, Range(1, 10)] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayers;
    [SerializeField] private Vector3 interactionOffset = new Vector3(0f, 1f, 0f);

    [Header("Debug (Odin)")]
    [ShowInInspector, ReadOnly]
    private IInteractable currentInteractable;

    [Header("Gizmos Settings")]
    [SerializeField] private Color gizmoColor = Color.cyan;
    [SerializeField] private Color sphereColor = new Color(0, 1, 0, 0.1f);
    [SerializeField, Range(0.1f, 0.5f)] private float sphereRadius = 0.3f;

    private Transform playerTransform;

    private string lastIdInteracted;
    

    private void Start()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindWithTag("PlayerModel");

        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Jogador não encontrado! Certifique-se de que existe um objeto com a tag 'PlayerModel' na cena.");
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        CheckForInteractables();
    }

    public void ForceCheckInteractables()
    {
        CheckForInteractables();
    }

    public string GetLastIdInteracted()
    {
        return lastIdInteracted;
    }

    public void UpdateLastId(string id)
    {
        lastIdInteracted = id;
    }

    private void CheckForInteractables()
    {
        Vector3 origin = playerTransform.position + interactionOffset;

        // usando OverlapSphereNonAlloc para melhor performance
        Collider[] hitColliders = new Collider[10];
        int numColliders = Physics.OverlapSphereNonAlloc(
            origin,
            interactionRange,
            hitColliders,
            interactableLayers,
            QueryTriggerInteraction.Collide 
        );

        IInteractable nearestInteractable = null;
        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < numColliders; i++)
        {
            Collider collider = hitColliders[i];
            if (collider.TryGetComponent(out IInteractable interactable))
            {
                Vector3 closestPoint = collider.ClosestPoint(origin);
                float distance = Vector3.Distance(origin, closestPoint);

                if (distance <= interactionRange &&
                   distance < closestDistance &&
                   interactable.IsInteractable())
                {
                    closestDistance = distance;
                    nearestInteractable = interactable;
                }
            }
        }

        if (nearestInteractable != null)
        {
            if (nearestInteractable != currentInteractable)
            {
                currentInteractable?.OnLostFocus();
                currentInteractable = nearestInteractable;
                currentInteractable.OnFocus();
            }
        }
        else
        {
            currentInteractable?.OnLostFocus();
            currentInteractable = null;
        }
    }

    [Button("Interagir (Debug)"), HideInEditorMode]
    public void TryInteract()
    {
        if (playerTransform == null) return;
        currentInteractable?.OnInteract();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enabled) return;

        Transform gizmoTransform = Application.isPlaying ?
            playerTransform :
            GameObject.FindWithTag("PlayerModel")?.transform;

        if (gizmoTransform == null) return;

        Vector3 origin = gizmoTransform.position + interactionOffset;

        // Desenha a esfera de interação
        Gizmos.color = sphereColor;
        Gizmos.DrawSphere(origin, interactionRange);

        // Desenha a esfera de interação (wireframe)
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(origin, interactionRange);

        // Desenha o ponto de origem
        Gizmos.DrawSphere(origin, sphereRadius);
    }
#endif
}