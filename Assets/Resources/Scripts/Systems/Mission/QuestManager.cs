using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
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
    private Dictionary<string, bool> completedTutorials = new Dictionary<string, bool>();

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
    public static Action<Quest> OnTutorialStarted { get; internal set; }

    [BoxGroup("Eventos")]
    [ShowInInspector, ReadOnly, PropertyOrder(201)]
    public static Action<QuestStep> OnTutorialStepChanged { get; internal set; }

    [BoxGroup("Eventos")]
    [ShowInInspector, ReadOnly, PropertyOrder(202)]
    public static Action<string> OnTutorialCompleted { get; internal set; }

    [BoxGroup("Navegação Atual")]
    [ShowInInspector, ReadOnly, PropertyOrder(50)]
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

        //por enquanto que nao tem save
#if UNITY_EDITOR
        foreach (var item in tutorialMissions)
        {
            item.isCompleted = false;
            foreach (var s in item.steps)
            {
                s.isCompleted = false;
            }
        }
#endif

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
        if(tutorialMissions != null) return false;

        return tutorialMissions.Find(m => m.questID == identifier).isCompleted == true;
    }

    public void StartMission(string missionID)
    {
        // Encontra a missão
        currentMission = tutorialMissions.Find(m => m.questID == missionID);

        if (currentMission == null || currentMission.isCompleted) return;

        // Começa com o primeiro passo não completado
        currentStep = currentMission.steps.Find(s => !s.isCompleted);

        if (currentStep == null) return;

        SetupStep(currentStep);
    }

    private void SetupStep(QuestStep step)
    {
        QuestUI.Instance.ShowInstruction(step.instructionText);

        // Configura indicadores visuais
        if (step.objective.objectiveType == QuestObjectiveType.ReachLocation)
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
        if (currentMission != null && currentStep.objective.objectiveType == QuestObjectiveType.CollectItem)
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
        SaveTutorialProgress(currentMission.questID);

        // Limpa a referência atual
        currentMission = null;
        currentStep = null;

        // Implemente qualquer feedback de conclusão
        OnTutorialCompleted?.Invoke("Missão Completa!");
    }
}