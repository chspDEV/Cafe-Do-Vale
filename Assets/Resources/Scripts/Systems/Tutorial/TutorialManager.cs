using System;
using System.Collections.Generic;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;
using UnityEngine.Rendering;

public class TutorialManager : Singleton<TutorialManager>
{
    [SerializeField] private List<TutorialMission> tutorialMissions;
    [SerializeField] private GameObject navigationArrowPrefab;

    private Dictionary<string, bool> completedTutorials = new Dictionary<string, bool>();

    private TutorialMission currentMission;
    public TutorialMission CurrentMission
    {
        get { return currentMission; }
        private set { currentMission = value; }
    }

    private TutorialStep currentStep;
    public TutorialStep CurrentStep
    {
        get { return currentStep; }
        private set { currentStep = value; }
    }

    public static Action<TutorialMission> OnTutorialStarted { get; internal set; }
    public static Action<TutorialStep> OnTutorialStepChanged { get; internal set; }
    public static Action<string> OnTutorialCompleted { get; internal set; }

    private GameObject currentArrow;

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        LoadTutorialProgress();
    }

    private void LoadTutorialProgress()
    {
        /* implementar asset nathan
        // Carrega o progresso salvo do PlayerPrefs ou de um save system
        foreach (var mission in tutorialMissions)
        {
            string key = "TUTORIAL_" + mission.missionID;
            mission.isCompleted = PlayerPrefs.GetInt(key, 0) == 1;
        }
        */
    }

    private void SaveTutorialProgress(string missionID)
    {
        /* implementar asset nathan
        PlayerPrefs.SetInt("TUTORIAL_" + missionID, 1);
        PlayerPrefs.Save();
        */
    }

    public bool IsMissionCompleted(string identifier)
    { 
        return tutorialMissions.Find(m => m.missionID == identifier).isCompleted == true;
    }

    public void StartMission(string missionID)
    {
        // Encontra a missão
        currentMission = tutorialMissions.Find(m => m.missionID == missionID);

        if (currentMission == null || currentMission.isCompleted) return;

        // Começa com o primeiro passo não completado
        currentStep = currentMission.steps.Find(s => !s.isCompleted);

        if (currentStep == null) return;

        SetupStep(currentStep);
    }

    private void SetupStep(TutorialStep step)
    {
        TutorialUI.Instance.ShowInstruction(step.instructionText);

        // Configura indicadores visuais
        if (step.objective.objectiveType == TutorialObjectiveType.ReachLocation)
        {
            CreateNavigationArrow(step.objective.targetPosition);
        }

        OnTutorialStepChanged?.Invoke(currentStep);
    }

    private void CreateNavigationArrow(Vector3 targetPosition)
    {
        if (currentArrow != null) Destroy(currentArrow);

        currentArrow = Instantiate(navigationArrowPrefab);
        var arrowScript = currentArrow.GetComponent<NavigationArrow>();

        // Configure a seta (ajuste conforme sua implementação)
        GameObject player = GameAssets.Instance.player;
        currentArrow.transform.position = new Vector3(currentArrow.transform.position.x, -1f, currentArrow.transform.position.z);
        arrowScript.player = player.transform;

        // Cria um objeto temporário para a posição alvo
        GameObject targetObj = new GameObject("ArrowTarget");
        targetObj.transform.position = targetPosition;
        arrowScript.target = targetObj.transform;
    }

    public void CheckItemCollected(string item_id)
    {
        if (currentMission != null && currentStep.objective.objectiveType == TutorialObjectiveType.CollectItem)
        {
            if (item_id == CurrentStep.objective.targetID)
            {
                CompleteCurrentStep();
            }
        }
        
    }

    public void CompleteCurrentStep()
    {
        if (currentStep == null) return;

        currentStep.isCompleted = true;

        // Limpa os indicadores visuais
        if (currentArrow != null)
        {
            Destroy(currentArrow);
            currentArrow = null;
        }

        // Verifica se há próximos passos
        var nextStep = currentMission.steps.Find(s => !s.isCompleted);

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

    public void CompleteCurrentMission()
    {
        currentMission.isCompleted = true;
        SaveTutorialProgress(currentMission.missionID);

        // Limpa a referência atual
        currentMission = null;
        currentStep = null;

        // Implemente qualquer feedback de conclusão
        OnTutorialCompleted?.Invoke("Missão Completa!");
    }
}