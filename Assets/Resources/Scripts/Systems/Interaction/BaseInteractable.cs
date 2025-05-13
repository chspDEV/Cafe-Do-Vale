using UnityEngine;
using UnityEngine.UI;
using Tcp4;
using Sirenix.OdinInspector;

public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    [TabGroup("Configura��es", "Interag�vel")]
    [LabelText("Habilitado")]
    [SerializeField] private bool isInteractable = true;

    [TabGroup("Configura��es", "Visual")]
    [BoxGroup("Configura��es/Visual/UI", ShowLabel = false)]
    [LabelText("Offset UI")]
    [SerializeField] protected Vector3 uiOffset = new Vector3(0, 1.5f, 0);

    [BoxGroup("Configura��es/Visual/UI")]
    [LabelText("Sprite de Intera��o")]
    [PreviewField(50)]
    [SerializeField] protected Sprite interactionSprite;

    [TabGroup("Configura��es", "F�sica")]
    [BoxGroup("Configura��es/F�sica/Colisor")]
    [LabelText("Colisor de Intera��o")]
    [SerializeField] private Collider interactionCollider;

    [BoxGroup("Configura��es/F�sica/Colisor")]
    [LabelText("Usar Trigger")]
    [SerializeField] private bool useTrigger = false;

    [TabGroup("Anima��o", "Escala")]
    [BoxGroup("Anima��o/Escala/Configs")]
    [LabelText("Escala Inicial")]
    [Tooltip("Tamanho inicial antes da anima��o")]
    [SerializeField] private Vector3 initialScale = Vector3.zero;

    [BoxGroup("Anima��o/Escala/Configs")]
    [LabelText("Escala Alvo")]
    [Tooltip("Tamanho final ap�s a anima��o")]
    [SerializeField] private Vector3 targetScale = Vector3.one;

    [BoxGroup("Anima��o/Escala/Configs")]
    [LabelText("Dura��o")]
    [Tooltip("Tempo para completar a anima��o")]
    [MinValue(0.1)]
    [SerializeField] private float growDuration = 0.3f;

    [TabGroup("Anima��o", "Bounce")]
    [BoxGroup("Anima��o/Bounce/Configs")]
    [LabelText("Escala M�nima")]
    [Tooltip("Menor tamanho durante o efeito")]
    [MinValue(0.1)]
    [SerializeField] private float bounceScaleMin = 0.9f;

    [BoxGroup("Anima��o/Bounce/Configs")]
    [LabelText("Escala M�xima")]
    [Tooltip("Maior tamanho durante o efeito")]
    [MinValue(0.1)]
    [SerializeField] private float bounceScaleMax = 1.1f;

    [BoxGroup("Anima��o/Bounce/Configs")]
    [LabelText("Velocidade")]
    [Tooltip("Rapidez do efeito de bounce")]
    [MinValue(0.1)]
    [SerializeField] private float bounceSpeed = 5f;

    [BoxGroup("Anima��o/Estado")]
    [ShowInInspector, ReadOnly]
    private float animationProgress;

    [BoxGroup("Anima��o/Estado")]
    [ShowInInspector, ReadOnly]
    private bool isGrowing;

    [BoxGroup("Anima��o/Estado")]
    [ShowInInspector, ReadOnly]
    private bool isBouncing;

    [BoxGroup("Anima��o/Estado")]
    [ShowInInspector, ReadOnly]
    private bool isFocused;

    [BoxGroup("Refer�ncias")]
    [SerializeField] protected Image interactionIndicator;
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