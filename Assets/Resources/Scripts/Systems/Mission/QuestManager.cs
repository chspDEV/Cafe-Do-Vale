using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Tcp4;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;
using UnityEngine.Rendering;
public class QuestManager : Singleton<QuestManager>
{
    [Title("Configurações de Missões")]
    [BoxGroup("Missões do Tutorial")]
    [SerializeField, Required, ListDrawerSettings(ShowPaging = true)]
    private List<Quest> tutorialMissions;
    [BoxGroup("Configurações de Navegação")]
    [SerializeField, Required, AssetsOnly, PreviewField(50)]
    private GameObject navigationArrowPrefab;
    [Title("Estado do Tutorial")]
    [BoxGroup("Estado do Tutorial")]
    [ShowInInspector, ReadOnly, PropertyOrder(100)]
    public readonly List<Quest> completedTutorials = new();
    [BoxGroup("Missão Atual")]
    [ShowInInspector, ReadOnly, PropertyOrder(1)]
    private Quest currentMission;
    public Quest CurrentMission
    {
        get { return currentMission; }
        private set { currentMission = value; }
    }
    [BoxGroup("Passo Atual")]
    [ShowInInspector, ReadOnly, PropertyOrder(2)]
    private QuestStep currentStep;
    public QuestStep CurrentStep
    {
        get { return currentStep; }
        private set { currentStep = value; }
    }
    [BoxGroup("Eventos")]
    [ShowInInspector, ReadOnly, PropertyOrder(200)]
    public static Action<Quest> OnQuestStarted { get; internal set; }
    [BoxGroup("Eventos")]
    [ShowInInspector, ReadOnly, PropertyOrder(201)]
    public static Action<QuestStep> OnQuestStepChanged { get; internal set; }
    [BoxGroup("Eventos")]
    [ShowInInspector, ReadOnly, PropertyOrder(202)]
    public static Action<string> OnQuestCompleted { get; internal set; }
    [BoxGroup("Navegação Atual")]
    [ShowInInspector, ReadOnly, PropertyOrder(50)]
    private GameObject currentArrow;
    private bool isProcessingStep = false;
    private bool isProcessingMission = false;
    private GameObject currentArrowTarget;
    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        LoadProgress();
    }
    private void LoadProgress()
    {
        //APENAS PARA TESTE DO JOGO!
#if UNITY_EDITOR
        if (tutorialMissions != null)
        {
            foreach (var item in tutorialMissions)
            {
                item.isCompleted = false;
                if (item.steps != null)
                {
                    foreach (var s in item.steps)
                    {
                        s.isCompleted = false;
                    }
                }
            }
        }
#endif
    }
    private void SaveProgress(string missionID)
    {
        var saveManager = FindFirstObjectByType<SaveManager>();
        if (saveManager != null)
        {
            Debug.Log($"[QuestManager] Saving progress for mission: {missionID}");
            saveManager.Save(); 
        }
        else
        {
            Debug.LogWarning("[QuestManager] SaveManager not found! Cannot save quest progress.");
        }
    }
    public bool IsMissionCompleted(string identifier)
    {
        if (tutorialMissions == null || string.IsNullOrEmpty(identifier))
            return false;
        Quest mission = tutorialMissions.FirstOrDefault(m => m.questID == identifier);
        return mission != null && mission.isCompleted;
    }
    public bool IsMissionStarted(string identifier)
    {
        if (tutorialMissions == null || string.IsNullOrEmpty(identifier))
            return false;
        Quest mission = tutorialMissions.FirstOrDefault(m => m.questID == identifier);
        return mission != null && !mission.isCompleted && mission.isStarted;
    }
    public void StartMission(string missionID)
    {
        if (string.IsNullOrEmpty(missionID) || tutorialMissions == null)
        {
            Debug.LogWarning($"[QuestManager] Cannot start mission: Invalid missionID or tutorialMissions is null");
            return;
        }
        if (isProcessingMission)
        {
            Debug.LogWarning($"[QuestManager] Already processing a mission, skipping start of {missionID}");
            return;
        }
        Quest targetMission = tutorialMissions.FirstOrDefault(m => m.questID == missionID);
        if (targetMission == null)
        {
            Debug.LogError($"[QuestManager] Mission not found: {missionID}");
            return;
        }
        if (targetMission.isCompleted)
        {
            Debug.LogWarning($"[QuestManager] Mission already completed: {missionID}");
            return;
        }
        CleanupCurrentMission();
        currentMission = targetMission;
        currentMission.isStarted = true;
        if (currentMission.steps == null || currentMission.steps.Count == 0)
        {
            Debug.LogError($"[QuestManager] Mission {missionID} has no steps!");
            CompleteCurrentMission();
            return;
        }
        QuestStep firstIncompleteStep = currentMission.steps.FirstOrDefault(s => !s.isCompleted);
        if (firstIncompleteStep == null)
        {
            Debug.LogWarning($"[QuestManager] All steps already completed for mission {missionID}");
            CompleteCurrentMission();
            return;
        }
        currentStep = firstIncompleteStep;
        SetupStep(currentStep);
        OnQuestStarted?.Invoke(currentMission);
        Debug.Log($"[QuestManager] Started mission: {missionID}");
    }
    private void SetupStep(QuestStep step)
    {
        if (step == null)
        {
            Debug.LogError("[QuestManager] Attempted to setup null step!");
            return;
        }
        if (QuestUI.Instance != null)
        {
            QuestUI.Instance.ShowInstruction(step.instructionText);
        }
        else
        {
            Debug.LogWarning("[QuestManager] QuestUI.Instance is null!");
        }
        if (step.objective != null && step.objective.objectiveType == QuestObjectiveType.ReachLocation)
        {
            CreateNavigationArrow(step.objective.targetPosition);
        }
        OnQuestStepChanged?.Invoke(step);
        Debug.Log($"[QuestManager] Setup step: {step.instructionText}");
    }
    private void CreateNavigationArrow(Vector3 targetPosition)
    {
        CleanupNavigationArrow();
        if (navigationArrowPrefab == null)
        {
            Debug.LogWarning("[QuestManager] navigationArrowPrefab is null!");
            return;
        }
        try
        {
            currentArrow = Instantiate(navigationArrowPrefab);
            var arrowScript = currentArrow.GetComponent<NavigationArrow>();
            if (arrowScript == null)
            {
                Debug.LogError("[QuestManager] NavigationArrow component not found on prefab!");
                Destroy(currentArrow);
                currentArrow = null;
                return;
            }
            if (GameAssets.Instance?.player == null)
            {
                Debug.LogWarning("[QuestManager] GameAssets.Instance.player is null!");
                Destroy(currentArrow);
                currentArrow = null;
                return;
            }
            GameObject player = GameAssets.Instance.player;
            currentArrow.transform.position = new Vector3(currentArrow.transform.position.x, -1f, currentArrow.transform.position.z);
            arrowScript.player = player.transform;
            currentArrowTarget = new GameObject("ArrowTarget_" + System.DateTime.Now.Ticks);
            currentArrowTarget.transform.position = targetPosition;
            arrowScript.target = currentArrowTarget.transform;
            Debug.Log($"[QuestManager] Created navigation arrow to position: {targetPosition}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[QuestManager] Error creating navigation arrow: {e.Message}");
            CleanupNavigationArrow();
        }
    }
    private void CleanupNavigationArrow()
    {
        if (currentArrow != null)
        {
            Destroy(currentArrow);
            currentArrow = null;
        }
        if (currentArrowTarget != null)
        {
            Destroy(currentArrowTarget);
            currentArrowTarget = null;
        }
    }
    private void CleanupCurrentMission()
    {
        CleanupNavigationArrow();
        isProcessingStep = false;
        isProcessingMission = false;
    }
    public void CheckItemCollected(string item_id)
    {
        if (string.IsNullOrEmpty(item_id) || currentMission == null || currentStep == null)
            return;
        if (currentStep.objective == null || currentStep.objective.objectiveType != QuestObjectiveType.CollectItem)
            return;
        if (item_id == currentStep.objective.targetID)
        {
            CompleteCurrentStep();
        }
    }
    [Button("Skip Current Step")]
    public void SkipCurrentStep()
    {
        if (currentStep != null && !isProcessingStep)
        {
            Debug.Log($"[QuestManager] Skipping step: {currentStep.instructionText}");
            CompleteCurrentStep();
        }
        else
        {
            Debug.LogWarning("[QuestManager] Cannot skip - no current step or already processing");
        }
    }
    public void CompleteCurrentStep()
    {
        if (currentStep == null || isProcessingStep)
        {
            Debug.LogWarning("[QuestManager] Cannot complete step - null or already processing");
            return;
        }
        isProcessingStep = true;
        try
        {
            Debug.Log($"[QuestManager] Completing step: {currentStep.instructionText}");
            currentStep.isCompleted = true;
            CleanupNavigationArrow();
            if (currentMission?.steps == null)
            {
                Debug.LogError("[QuestManager] Current mission or steps is null!");
                return;
            }
            QuestStep nextStep = currentMission.steps.FirstOrDefault(s => !s.isCompleted);
            if (nextStep != null)
            {
                currentStep = nextStep;
                SetupStep(currentStep);
            }
            else
            {
                CompleteCurrentMission();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[QuestManager] Error completing step: {e.Message}");
        }
        finally
        {
            isProcessingStep = false;
        }
    }
    public void CompleteCurrentMission()
    {
        if (currentMission == null || isProcessingMission)
        {
            Debug.LogWarning("[QuestManager] Cannot complete mission - null or already processing");
            return;
        }
        isProcessingMission = true;
        try
        {
            Debug.Log($"[QuestManager] Completing mission: {currentMission.questID}");
            currentMission.isCompleted = true;
            completedTutorials.Add(currentMission);
            SaveProgress(currentMission.questID);
            Quest completedMission = currentMission;
            currentMission = null;
            currentStep = null;
            CleanupCurrentMission();
            OnQuestCompleted?.Invoke($"Missão {completedMission.questID} Completa!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[QuestManager] Error completing mission: {e.Message}");
        }
        finally
        {
            isProcessingMission = false;
        }
    }
    [Button("Force Cleanup")]
    public void ForceCleanup()
    {
        CleanupCurrentMission();
        currentMission = null;
        currentStep = null;
        Debug.Log("[QuestManager] Forced cleanup completed");
    }
    [Button("Debug Current State")]
    public void DebugCurrentState()
    {
        Debug.Log($"[QuestManager DEBUG] " +
                  $"Current Mission: {(currentMission?.questID ?? "NULL")} | " +
                  $"Current Step: {(currentStep?.instructionText ?? "NULL")} | " +
                  $"Processing Step: {isProcessingStep} | " +
                  $"Processing Mission: {isProcessingMission} | " +
                  $"Arrow Active: {(currentArrow != null)} | " +
                  $"Arrow Target Active: {(currentArrowTarget != null)}");
    }
    private void OnDestroy()
    {
        CleanupCurrentMission();
    }
    public List<Quest>  GetTutorialMissions()
    {
        return tutorialMissions;
    }
}