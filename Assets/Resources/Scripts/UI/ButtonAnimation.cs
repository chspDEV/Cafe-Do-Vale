using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(EventTrigger))]
public class ButtonAnimation : MonoBehaviour
{
    [Header("Configuracoes de Animacao")]
    [SerializeField] private float scaleFactor = 1.1f;
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private Ease easeType = Ease.OutBack;

    [Header("Animacao de Click")]
    [SerializeField] private Vector3 punchScale = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] private int punchVibrato = 5;
    [SerializeField] private float punchElasticity = 0.5f;

    private Vector3 originalScale;
    private EventTrigger eventTrigger;

    private void Awake()
    {
        originalScale = transform.localScale;
        eventTrigger = GetComponent<EventTrigger>();
        SetupEventTriggers();
    }

    private void SetupEventTriggers()
    {
        AddEventTriggerListener(EventTriggerType.PointerEnter, OnPointerEnter);
        AddEventTriggerListener(EventTriggerType.PointerExit, OnPointerExit);
        AddEventTriggerListener(EventTriggerType.Select, OnSelect);
        AddEventTriggerListener(EventTriggerType.Deselect, OnDeselect);
        AddEventTriggerListener(EventTriggerType.PointerClick, OnClick);
    }

    private void AddEventTriggerListener(EventTriggerType eventType, UnityAction action)
    {
        var entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener((eventData) => action.Invoke());
        eventTrigger.triggers.Add(entry);
    }

    // --- Metodos de Resposta a Eventos ---

    public void OnPointerEnter() => AnimateScaleUp();
    public void OnPointerExit() => AnimateScaleDown();
    public void OnSelect() => AnimateScaleUp();
    public void OnDeselect() => AnimateScaleDown();

    public void OnClick()
    {
        /*
        var sfxConfig = new AudioClipConfig
        {
            Category = AudioCategory.SFX,
            AudioID = "ui_select",
            VolumeScale = 1f,
        };
        AudioEventChannel.RaisePlayAudio(sfxConfig);
        */

        transform.DOKill(true);
        transform.DOPunchScale(punchScale, animationDuration, punchVibrato, punchElasticity);
    }

    // --- Metodos de Logica da Animacao ---

    private void AnimateScaleUp()
    {
        /*
        var sfxConfig = new AudioClipConfig
        {
            Category = AudioCategory.SFX,
            AudioID = "ui_hover",
            VolumeScale = 1f,
        };
        AudioEventChannel.RaisePlayAudio(sfxConfig);
        */

        transform.DOKill(true);
        transform.DOScale(originalScale * scaleFactor, animationDuration).SetEase(easeType);
    }

    private void AnimateScaleDown()
    {
        transform.DOKill(true);
        transform.DOScale(originalScale, animationDuration).SetEase(easeType);
    }
}