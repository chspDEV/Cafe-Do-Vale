using UnityEngine;
using UnityEngine.UI;
using Tcp4;

public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    [Header("UI Settings")]
    [SerializeField] protected Vector3 uiOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] protected Sprite interactionSprite;

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

    protected virtual void Start()
    {
        CreateInteractionIndicator();
        InitializeBillboard();
        ApplyInitialScale();
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

    private void Update()
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

        // Oscilação suave entre as escalas mínima e máxima
        float t = (Mathf.Sin(animationProgress) + 1f) / 2f; // Normalizado para 0-1
        float currentScale = Mathf.Lerp(bounceScaleMin, bounceScaleMax, t);

        interactionIndicator.transform.localScale = targetScale * currentScale;
    }

    public virtual void OnFocus()
    {
        isFocused = true;

        if (interactionIndicator != null)
        {
            interactionIndicator.enabled = true;
            interactionIndicator.transform.localScale = initialScale;
            isGrowing = true;
            isBouncing = false;
            animationProgress = 0f;
        }
    }

    public virtual void OnInteract()
    {
        Debug.Log($"Interagiu com {name}");
    }

    public virtual void OnLostFocus()
    {
        isFocused = false;

        if (interactionIndicator != null)
        {
            interactionIndicator.enabled = false;
            interactionIndicator.transform.localScale = initialScale;
        }
    }

    private void OnDestroy()
    {
        if (interactionIndicator != null)
        {
            Destroy(interactionIndicator.gameObject);
        }
    }
}