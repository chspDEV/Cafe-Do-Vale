using UnityEngine;
using UnityEngine.UI;
using Tcp4;

public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    [Header("UI Settings")]
    [SerializeField] protected Vector3 uiOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] protected Sprite interactionSprite;

    [Header("Animation Settings")]
    [SerializeField] private float growDuration = 0.3f;
    [SerializeField] private float bounceIntensity = 0.1f;
    [SerializeField] private float bounceSpeed = 5f;

    protected Image interactionIndicator;
    private Vector3 initialScale;
    private float animationProgress;
    private bool isGrowing;
    private bool isBouncing;
    private bool isFocused;

    protected virtual void Start()
    {
        CreateInteractionIndicator();
        InitializeBillboard();
        initialScale = interactionIndicator.transform.localScale;
        interactionIndicator.transform.localScale = Vector3.zero;
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
            else
            {
                Debug.LogWarning("Billboard component não encontrado no indicador de interação!");
            }
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
            Vector3.zero,
            initialScale,
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
        float scaleFactor = 1 + Mathf.Sin(animationProgress) * bounceIntensity;
        interactionIndicator.transform.localScale = initialScale * scaleFactor;
    }

    public virtual void OnFocus()
    {
        isFocused = true;

        if (interactionIndicator != null)
        {
            interactionIndicator.enabled = true;
            interactionIndicator.transform.localScale = Vector3.zero;
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
            interactionIndicator.transform.localScale = Vector3.zero;
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