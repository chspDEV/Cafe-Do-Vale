using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using PlugInputPack;
using Tcp4.Assets.Resources.Scripts.Managers;

public class QuestUI : Singleton<QuestUI>
{
    [Title("Configurações Principais")]
    [BoxGroup("Configurações de Exibição")]
    [SerializeField] private float displayDuration = 60 * 30f;

    [Title("Referências de UI")]
    [BoxGroup("Referências de UI")]
    [Required][SerializeField] private GameObject questPanel;
    [BoxGroup("Referências de UI")]
    [Required][SerializeField] private TextMeshProUGUI instructionText;
    [BoxGroup("Referências de UI")]
    [Required][SerializeField] private Image progressFill;
    [BoxGroup("Referências de UI")]
    [Required][SerializeField] private GameObject completionMarkerPrefab;
    [BoxGroup("Referências de UI")]
    [Required][SerializeField] private GameObject nextIndicator;
    [BoxGroup("Referências de UI")]
    [Required][SerializeField] private Transform progressContainer;

    [Title("Configurações de Animação")]
    [BoxGroup("Configurações de Animação")]
    [Range(0.1f, 2f)][SerializeField] private float fadeDuration = 0.5f;
    [BoxGroup("Configurações de Animação")]
    [Required][SerializeField] private CanvasGroup canvasGroup;

    [Title("Estado Interno")]
    [BoxGroup("Estado Interno")]
    [ShowInInspector][ReadOnly] private bool isShowingInstruction;
    [BoxGroup("Estado Interno")]
    [ShowInInspector][ReadOnly] private Coroutine currentDisplayRoutine;

    [Title("Configurações de Controle")]
    [SerializeField] private PlugInputComponent playerInputs;
    [SerializeField] private float holdDuration = 2f;
    [SerializeField] private Image holdFillImage;

    private float holdTimer = 0f;
    private bool isHoldingNext = false;

    private QuestChecker questChecker;

    public override void Awake()
    {
        base.Awake();

        if (playerInputs == null)
            playerInputs = GameAssets.Instance.inputComponent;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>() ?? questPanel.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0;
        questPanel.SetActive(false);

        if (nextIndicator != null)
            nextIndicator.SetActive(false);


        if (holdFillImage != null)
            holdFillImage.fillAmount = 0f;

        questChecker = FindFirstObjectByType<QuestChecker>();
    }

    private void Update()
    {
        // Se não há missão ativa, desativa indicador e para tudo
        if (QuestManager.Instance.CurrentMission == null)
        {
            if (nextIndicator != null) nextIndicator.SetActive(false);
            return;
        }

        // Verifica se o jogador está pressionando o botão de "próximo"
        bool holdPressed = playerInputs["HoldMission"].IsPressed;

        if (holdPressed)
        {
            holdTimer += Time.deltaTime;
            isHoldingNext = true;

            if (holdFillImage != null)
                holdFillImage.fillAmount = Mathf.Clamp01(holdTimer / holdDuration);

            if (holdTimer >= holdDuration)
            {
                NextQuest();
                holdTimer = 0f;

                if (holdFillImage != null)
                    holdFillImage.fillAmount = 0f;
            }
        }
        else if (isHoldingNext)
        {
            // Reset ao soltar
            holdTimer = 0f;
            isHoldingNext = false;

            if (holdFillImage != null)
                holdFillImage.fillAmount = 0f;
        }
    }


    private void OnEnable()
    {
        QuestManager.OnQuestStarted += OnQuestStarted;
        QuestManager.OnQuestStepChanged += OnStepChanged;
        QuestManager.OnQuestCompleted += OnQuestCompleted;
    }

    private void OnDisable()
    {
        QuestManager.OnQuestStarted -= OnQuestStarted;
        QuestManager.OnQuestStepChanged -= OnStepChanged;
        QuestManager.OnQuestCompleted -= OnQuestCompleted;
    }

    public void ShowInstruction(string instruction, bool isMissionTitle = false)
    {
        if (questChecker != null)
            instruction = questChecker.ReplaceButtonTags(instruction);

        if (currentDisplayRoutine != null)
            StopCoroutine(currentDisplayRoutine);

        // ✅ Ativa o "Next" somente se for o título da missão
        nextIndicator?.SetActive(isMissionTitle);

        currentDisplayRoutine = StartCoroutine(DisplayInstructionRoutine(instruction));
    }


    private IEnumerator DisplayInstructionRoutine(string instruction)
    {
        isShowingInstruction = true;

        // Ativa e faz fade in
        questPanel.SetActive(true);
        instructionText.text = instruction;

        yield return StartCoroutine(FadePanel(0, 1));

        // Mostra por um tempo ou até interação
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

    private void OnQuestStarted(Quest mission)
    {
        UpdateProgressUI(mission);
        ShowInstruction($"Missão: {mission.questName}", isMissionTitle: true);
    }


    private void OnStepChanged(QuestStep step)
    {
        if (QuestManager.Instance.CurrentMission != null)
            UpdateProgressUI(QuestManager.Instance.CurrentMission);

        ShowInstruction(step.instructionText, isMissionTitle: false);
    }


    private void OnQuestCompleted(string questId)
    {
        CompleteProgressUI();
        ShowInstruction("Missão completa!");
        StartCoroutine(CompleteQuestRoutine());
    }

    private IEnumerator CompleteQuestRoutine()
    {
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FadePanel(1, 0));
        questPanel.SetActive(false);
    }

    private void UpdateProgressUI(Quest mission)
    {
        foreach (Transform child in progressContainer)
            Destroy(child.gameObject);

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
        isShowingInstruction = false;
        QuestManager.Instance.CompleteCurrentStep();
    }

    public void SkipQuest()
    {
        if (QuestManager.Instance.CurrentMission != null)
        {
            QuestManager.Instance.CompleteCurrentMission();
            StartCoroutine(FadePanel(canvasGroup.alpha, 0));
            questPanel.SetActive(false);
        }
    }

    public void NextQuest()
    {
        if (QuestManager.Instance.CurrentMission != null &&
            QuestManager.Instance.CurrentStep != null)
        {
            QuestManager.Instance.CompleteCurrentStep();

            holdTimer = 0f;
            isHoldingNext = false;
            if (holdFillImage != null)
                holdFillImage.fillAmount = 0f;
        }
    }
}
