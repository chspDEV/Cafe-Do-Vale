using UnityEngine;
using Tcp4.Assets.Resources.Scripts.Managers;
using Tcp4;
using System;
using System.Collections.Generic;
using PlugInputPack;

public class QuestChecker : MonoBehaviour
{
    [SerializeField] private PlugInputComponent playerInputs;
    [SerializeField] private PlayerMovement playerMovement;

    // Dicionário para conversão dinâmica de nomes de botões
    private readonly Dictionary<CurrentInputType, Dictionary<string, string>> inputMappings
        = new Dictionary<CurrentInputType, Dictionary<string, string>>
    {
        {
            CurrentInputType.PC, new Dictionary<string, string>
            {
                { "Space", "SPACE" },
                { "LeftButton", "MOUSE ESQ" },
                { "RightButton", "MOUSE DIR" },
                { "W", "W" },
                { "A", "A" },
                { "S", "S" },
                { "D", "D" },
                { "MAP", "M" },
                { "RECIPE", "R" },
            }
        },
        {
            CurrentInputType.XBOX, new Dictionary<string, string>
            {
                { "Button South", "A" },
                { "Button East", "B" },
                { "Button North", "Y" },
                { "Button West", "X" },
                { "Left Trigger", "LT" },
                { "Right Trigger", "RT" },
                { "Left Shoulder", "LB" },
                { "W", "LS para frente" },
                { "A", "LS para esquerda" },
                { "S", "LS para trás" },
                { "D", "LS para direita" },
                { "MAP", "Y" },
                { "RECIPE", "X" },
            }
        },
        {
            CurrentInputType.PLAYSTATION, new Dictionary<string, string>
            {
                { "Button South", "✕" },
                { "Button East", "◯" },
                { "Button North", "▲" },
                { "Button West", "⬜" },
                { "Left Trigger", "L2" },
                { "Right Trigger", "R2" },
                { "Left Shoulder", "L1" },
                { "Right Shoulder", "R1" },
                { "W", "LS para frente" },
                { "A", "LS para esquerda" },
                { "S", "LS para trás" },
                { "D", "LS para direita" },
                { "MAP", "⬜" },
                { "RECIPE", "▲" },
            }
        },
    };

    private void Awake()
    {
        InitializeInputActions();
    }

    private void InitializeInputActions()
    {
        if (playerInputs == null)
            playerInputs = GameAssets.Instance.inputComponent;

        if (playerMovement == null)
            playerMovement = GameAssets.Instance.player.GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        var currentStep = QuestManager.Instance.CurrentStep;
        if (currentStep == null) return;

        var objective = currentStep.objective;
        if (objective == null) return;

        switch (objective.objectiveType)
        {
            case QuestObjectiveType.PressButton:
                CheckButtonPressObjective(objective);
                break;
            case QuestObjectiveType.ReachLocation:
                CheckLocationObjective(objective);
                break;
            case QuestObjectiveType.InteractWithObject:
                CheckInteractionObjective(objective);
                break;
        }
    }

    private void CheckInteractionObjective(QuestObjective objective)
    {
        if (InteractionManager.Instance.GetLastIdInteracted() == objective.targetID.ToString())
        {
            QuestManager.Instance.CompleteCurrentStep();
        }
    }

    private void CheckButtonPressObjective(QuestObjective objective)
    {
        if (IsMenuBlockingTutorial()) return;

        if (IsInputPressed(objective.requiredInput))
        {
            QuestManager.Instance.CompleteCurrentStep();
        }
    }

    private bool IsInputPressed(InputsPossibilities input)
    {
        return input switch
        {
            InputsPossibilities.Forward => playerMovement.forwardPressed,
            InputsPossibilities.Backward => playerMovement.backwardPressed,
            InputsPossibilities.Left => playerMovement.leftPressed,
            InputsPossibilities.Right => playerMovement.rightPressed,
            InputsPossibilities.Map => playerInputs["Map"].IsPressed,
            InputsPossibilities.Recipe => playerInputs["Recipe"].IsPressed,
            InputsPossibilities.Pause => playerInputs["Pause"].IsPressed,
            InputsPossibilities.Inventory => playerInputs["SeedInventory"].IsPressed,
            _ => false,
        };
    }

    private bool IsMenuBlockingTutorial()
    {
        return UIManager.Instance != null && UIManager.Instance.HasMenuOpen();
    }

    private void CheckLocationObjective(QuestObjective objective)
    {
        if (GameAssets.Instance.player == null) return;

        float distance = Vector3.Distance(
            GameAssets.Instance.player.transform.position,
            objective.targetPosition
        );

        if (distance <= objective.radius)
        {
            QuestManager.Instance.CompleteCurrentStep();
        }
    }

    /// <summary>
    /// Retorna o nome do botão adequado para a plataforma atual.
    /// </summary>
    public string GetButtonDisplayName(string actionName)
    {
        var currentType = GameAssets.Instance.currentInputType;
        if (inputMappings.TryGetValue(currentType, out var mapping))
        {
            if (mapping.TryGetValue(actionName, out var displayName))
                return displayName;
        }
        return actionName.ToUpper();
    }

    /// <summary>
    /// Substitui tags do tipo [BTN:Space] em um texto por nomes corretos para o input atual.
    /// </summary>
    public string ReplaceButtonTags(string text)
    {
        var currentType = GameAssets.Instance.currentInputType;

        if (!inputMappings.ContainsKey(currentType))
            return text;

        foreach (var mapping in inputMappings[currentType])
        {
            string tag = $"[BTN:{mapping.Key}]";
            if (text.Contains(tag))
                text = text.Replace(tag, mapping.Value);
        }

        return text;
    }
}
