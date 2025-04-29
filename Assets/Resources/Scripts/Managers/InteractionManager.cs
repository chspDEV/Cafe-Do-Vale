using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class InteractionManager : Singleton<InteractionManager>
{
    [Header("Configurações")]
    [SerializeField, Range(1, 10)] 
    private float interactionRange = 3f;

    [Header("Debug (Odin)")]
    [ShowInInspector, ReadOnly] 
    private IInteractable currentInteractable;

    [Header("Gizmos Settings")]
    [SerializeField] private Color gizmoColor = Color.cyan;
    [SerializeField] private Color hitColor = Color.yellow;
    [SerializeField, Range(0.1f, 0.5f)] private float sphereRadius = 0.3f;

    private Transform playerTransform; 

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("PlayerModel");
        
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Jogador não encontrado! Certifique-se de que existe um objeto com a tag 'Player' na cena.");
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        CheckForInteractables();
    }

    private void CheckForInteractables()
    {
        Ray ray = new Ray(playerTransform.position + new Vector3(0f,1f,0f), playerTransform.forward);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, interactionRange);

        if (hitSomething && hit.collider.TryGetComponent(out IInteractable interactable))
        {
            if (interactable != currentInteractable)
            {
                currentInteractable?.OnLostFocus();
                currentInteractable = interactable;
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

        // Tenta encontrar o jogador na cena se não estiver em Play Mode
        Transform gizmoTransform = Application.isPlaying ? 
            playerTransform : 
            GameObject.FindWithTag("Player")?.transform;

        if (gizmoTransform == null) return;

        // Configura as posições e direção
        Vector3 start = gizmoTransform.position  + new Vector3(0f,1f,0f);
        Vector3 direction = gizmoTransform.forward;
        float maxDistance = interactionRange;

        // Desenha a linha principal
        Gizmos.color = gizmoColor;
        Gizmos.DrawRay(start, direction * maxDistance);

        // Verica colisão apenas para o Gizmo
        if (Physics.Raycast(start, direction, out RaycastHit hit, maxDistance))
        {
            Gizmos.color = hitColor;
            Gizmos.DrawSphere(hit.point, sphereRadius);
        }

        // Desenha uma esfera no final do alcance
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(start + direction * maxDistance, sphereRadius);
    }
#endif
}