using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;

public class QuestUI : Singleton<QuestUI>
{
    [Title("Configura��es Principais")]
    [BoxGroup("Configura��es de Exibi��o")]
    [SerializeField] private float displayDuration = 60 * 30f;

    [Title("Refer�ncias de UI")]
    [BoxGroup("Refer�ncias de UI")]
    [Required][SerializeField] private GameObject tutorialPanel;
    [BoxGroup("Refer�ncias de UI")]
    [Required][SerializeField] private TextMeshProUGUI instructionText;
    [BoxGroup("Refer�ncias de UI")]
    [Required][SerializeField] private Image progressFill;
    [BoxGroup("Refer�ncias de UI")]
    [Required][SerializeField] private GameObject completionMarkerPrefab;
    [BoxGroup("Refer�ncias de UI")]
    [Required][SerializeField] private Transform progressContainer;
    [BoxGroup("Refer�ncias de UI")]
    [Required][SerializeField] private Button skipButton;
    [BoxGroup("Refer�ncias de UI")]
    [Required][SerializeField] private Button nextButton;

    [Title("Configura��es de Anima��o")]
    [BoxGroup("Configura��es de Anima��o")]
    [Range(0.1f, 2f)][SerializeField] private float fadeDuration = 0.5f;
    [BoxGroup("Configura��es de Anima��o")]
    [Required][SerializeField] private CanvasGroup canvasGroup;

    [Title("Estado Interno")]
    [BoxGroup("Estado Interno")]
    [ShowInInspector][ReadOnly] private bool isShowingInstruction;
    [BoxGroup("Estado Interno")]
    [ShowInInspector][ReadOnly] private Coroutine currentDisplayRoutine;

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
        QuestManager.OnTutorialStarted += OnTutorialStarted;
        QuestManager.OnTutorialStepChanged += OnStepChanged;
        QuestManager.OnTutorialCompleted += OnTutorialCompleted;
    }

    private void OnDisable()
    {
        QuestManager.OnTutorialStarted -= OnTutorialStarted;
        QuestManager.OnTutorialStepChanged -= OnStepChanged;
        QuestManager.OnTutorialCompleted -= OnTutorialCompleted;
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

        // Mostra por um tempo ou at� intera��o
        float elapsed = 0;
        while (elapsed < displayDuration && isShowingInstruction)
        {
            elapsed += Time.deltaTime;
            yield return null;
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

    private void OnTutorialStarted(Quest mission)
    {
        // Atualiza a UI para mostrar o progresso
        UpdateProgressUI(mission);

        // Mostra o nome da miss�o como primeira instru��o
        ShowInstruction($"Tutorial: {mission.questName}");
    }

    private void OnStepChanged(QuestStep step)
    {
        if(QuestManager.Instance.CurrentMission != null)
            UpdateProgressUI(QuestManager.Instance.CurrentMission);

        ShowInstruction(step.instructionText);

        // Mostra ou esconde o bot�o "Next" dependendo do tipo de objetivo
        nextButton.gameObject.SetActive(step.objective.objectiveType == QuestObjectiveType.InfoOnly);
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

    private void UpdateProgressUI(Quest mission)
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
            QuestManager.Instance.CompleteCurrentStep();
        }
    }

    public void SkipTutorial()
    {
        if (QuestManager.Instance.CurrentMission != null)
        {
            QuestManager.Instance.CompleteCurrentMission();
            StartCoroutine(FadePanel(canvasGroup.alpha, 0));
            tutorialPanel.SetActive(false);
        }
    }
}