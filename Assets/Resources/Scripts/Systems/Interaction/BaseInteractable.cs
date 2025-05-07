using UnityEngine;
using UnityEngine.UI;
using Tcp4;

public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    [Header("Interactable Settings")]
    [SerializeField] private bool isInteractable = true;

    [Header("UI Settings")]
    [SerializeField] protected Vector3 uiOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] protected Sprite interactionSprite;

    [Header("Collider Settings")]
    [SerializeField] private Collider interactionCollider;
    [SerializeField] private bool useTrigger = false;

    [Header("Scale Animation")]
    [Tooltip("Escala inicial antes da animação")]
    [SerializeField] private Vector3 initialScale = Vector3.zero;

    [Tooltip("Escala alvo após o crescimento")]
    [SerializeField] private Vector3 targetScale = Vector3.one;

    [Space]
    [Tooltip("Duração da animação de crescimento")]
    [SerializeField] private float growDuration = 0.3f;

    [Header("Bounce Animation")]
    [Tooltip("Escala mínima durante o bounce (relativo ao targetScale)")]
    [SerializeField] private float bounceScaleMin = 0.9f;

    [Tooltip("Escala máxima durante o bounce (relativo ao targetScale)")]
    [SerializeField] private float bounceScaleMax = 1.1f;

    [Tooltip("Velocidade da animação de bounce")]
    [SerializeField] private float bounceSpeed = 5f;

    protected Image interactionIndicator;
    private float animationProgress;
    private bool isGrowing;
    private bool isBouncing;
    private bool isFocused;

    public virtual void Start()
    {
        CreateInteractionIndicator();
        InitializeBillboard();
        ApplyInitialScale();
    }

    #region Interface Implementation
    public bool IsInteractable() => isInteractable;

    public virtual void OnFocus()
    {
        if (!IsInteractable()) return;

        isFocused = true;
        EnableUI();
    }

    public virtual void OnInteract()
    {
        if (!IsInteractable()) return;
        Debug.Log($"Interagiu com {name}");
    }

    public virtual void OnLostFocus()
    {
        isFocused = false;
        DisableUI();
    }
    #endregion

    #region Interaction Control
    public void EnableInteraction()
    {
        isInteractable = true;
        EnableUI();
    }

    public void DisableInteraction()
    {
        isInteractable = false;
        DisableUI();
        OnLostFocus();
    }
    #endregion

    #region UI Management
    private void EnableUI()
    {
        if (interactionIndicator == null) return;

        interactionIndicator.enabled = true;
        interactionIndicator.transform.localScale = initialScale;
        isGrowing = true;
        isBouncing = false;
        animationProgress = 0f;
    }

    private void DisableUI()
    {
        if (interactionIndicator == null) return;

        interactionIndicator.enabled = false;
        interactionIndicator.transform.localScale = initialScale;
    }

    private void CreateInteractionIndicator()
    {
        interactionIndicator = UIManager.Instance.PlaceImage(transform);

        if (interactionIndicator != null)
        {
            interactionIndicator.sprite = interactionSprite;
            interactionIndicator.enabled = false;
        }
    }

    private void InitializeBillboard()
    {
        if (interactionIndicator != null)
        {
            Billboard billboard = interactionIndicator.GetComponent<Billboard>();
            if (billboard != null)
            {
                billboard.positionOffset = uiOffset;
            }
        }
    }

    private void ApplyInitialScale()
    {
        if (interactionIndicator != null)
        {
            interactionIndicator.transform.localScale = initialScale;
        }
    }
    #endregion

    #region Animation
    public virtual void Update()
    {
        if (!isFocused || interactionIndicator == null) return;

        HandleAnimation();
    }

    private void HandleAnimation()
    {
        if (isGrowing)
        {
            UpdateGrowAnimation();
        }
        else if (isBouncing)
        {
            UpdateBounceAnimation();
        }
    }

    private void UpdateGrowAnimation()
    {
        animationProgress += Time.deltaTime / growDuration;
        interactionIndicator.transform.localScale = Vector3.Lerp(
            initialScale,
            targetScale,
            animationProgress
        );

        if (animationProgress >= 1f)
        {
            isGrowing = false;
            isBouncing = true;
            animationProgress = 0f;
        }
    }

    private void UpdateBounceAnimation()
    {
        animationProgress += Time.deltaTime * bounceSpeed;

        float t = (Mathf.Sin(animationProgress) + 1f) / 2f; 
        float currentScale = Mathf.Lerp(bounceScaleMin, bounceScaleMax, t);

        interactionIndicator.transform.localScale = targetScale * currentScale;
    }
    #endregion

    private void OnDestroy()
    {
        if (interactionIndicator != null)
        {
            Destroy(interactionIndicator.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (useTrigger && other.CompareTag("Player"))
        {
            InteractionManager.Instance.ForceCheckInteractables();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (useTrigger && other.CompareTag("Player"))
        {
            InteractionManager.Instance.ForceCheckInteractables();
        }
    }
}