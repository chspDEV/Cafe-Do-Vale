using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;
using System.Reflection;

public class TutorialUI : Singleton<TutorialUI>
{
    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private Image progressFill;
    [SerializeField] private GameObject completionMarkerPrefab;
    [SerializeField] private Transform progressContainer;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private float displayDuration = 3f;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private CanvasGroup canvasGroup;

    private bool isShowingInstruction;
    private Coroutine currentDisplayRoutine;

    public override void Awake()
    {
        base.Awake();

        if(canvasGroup == null)
        canvasGroup = GetComponent<CanvasGroup>() ?? tutorialPanel.AddComponent<CanvasGroup>();

        skipButton.onClick.AddListener(SkipTutorial);
        nextButton.onClick.AddListener(CompleteCurrentStep);

        // Inicialmente escondido
        canvasGroup.alpha = 0;
        tutorialPanel.SetActive(false);
    }

    private void OnEnable()
    {
        TutorialManager.OnTutorialStarted += OnTutorialStarted;
        TutorialManager.OnTutorialStepChanged += OnStepChanged;
        TutorialManager.OnTutorialCompleted += OnTutorialCompleted;
    }

    private void OnDisable()
    {
        TutorialManager.OnTutorialStarted -= OnTutorialStarted;
        TutorialManager.OnTutorialStepChanged -= OnStepChanged;
        TutorialManager.OnTutorialCompleted -= OnTutorialCompleted;
    }

    public void ShowInstruction(string instruction)
    {
        if (currentDisplayRoutine != null)
        {
            StopCoroutine(currentDisplayRoutine);
        }

        currentDisplayRoutine = StartCoroutine(DisplayInstructionRoutine(instruction));
    }

    private IEnumerator DisplayInstructionRoutine(string instruction)
    {
        isShowingInstruction = true;

        // Ativa e faz fade in
        tutorialPanel.SetActive(true);
        instructionText.text = instruction;

        yield return StartCoroutine(FadePanel(0, 1));

        // Mostra por um tempo ou até interação
        float elapsed = 0;
        while (elapsed < displayDuration && isShowingInstruction)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Fade out se não houver interação
        if (isShowingInstruction)
        {
            yield return StartCoroutine(FadePanel(250, 0));
            tutorialPanel.SetActive(false);
        }

        isShowingInstruction = false;
    }

    private IEnumerator FadePanel(float from, float to)
    {
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = to;
    }

    private void OnTutorialStarted(TutorialMission mission)
    {
        // Atualiza a UI para mostrar o progresso
        UpdateProgressUI(mission);

        // Mostra o nome da missão como primeira instrução
        ShowInstruction($"Tutorial: {mission.missionName}");
    }

    private void OnStepChanged(TutorialStep step)
    {
        if(TutorialManager.Instance.CurrentMission != null)
            UpdateProgressUI(TutorialManager.Instance.CurrentMission);

        ShowInstruction(step.instructionText);

        // Mostra ou esconde o botão "Next" dependendo do tipo de objetivo
        nextButton.gameObject.SetActive(step.objective.objectiveType == TutorialObjectiveType.InfoOnly);
    }

    private void OnTutorialCompleted(string missionId)
    {
        CompleteProgressUI();
        ShowInstruction("Tutorial completo!");
        StartCoroutine(CompleteTutorialRoutine());
    }

    private IEnumerator CompleteTutorialRoutine()
    {
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FadePanel(1, 0));
        tutorialPanel.SetActive(false);
    }

    private void UpdateProgressUI(TutorialMission mission)
    {
        // Limpa marcadores antigos
        foreach (Transform child in progressContainer)
        {
            Destroy(child.gameObject);
        }

        // Atualiza barra de progresso
        if (mission.steps.Count > 0)
        {
            int completedSteps = mission.steps.Count(s => s.isCompleted);
            progressFill.fillAmount = (float)completedSteps / mission.steps.Count;
        }

    }

    private void CompleteProgressUI()
    {
        progressFill.fillAmount = 1f;
    }

    public void CompleteCurrentStep()
    {
        if (isShowingInstruction)
        {
            isShowingInstruction = false;
            TutorialManager.Instance.CompleteCurrentStep();
        }
    }

    public void SkipTutorial()
    {
        if (TutorialManager.Instance.CurrentMission != null)
        {
            TutorialManager.Instance.CompleteCurrentMission();
            StartCoroutine(FadePanel(canvasGroup.alpha, 0));
            tutorialPanel.SetActive(false);
        }
    }
}