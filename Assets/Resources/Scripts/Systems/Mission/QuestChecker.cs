using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Tcp4.Assets.Resources.Scripts.Managers;
using Tcp4;
using System;

public class QuestChecker : MonoBehaviour
{
    [SerializeField] private PlayerInputs playerInputs;
    [SerializeField] private PlayerMovement playerMovement;

    private void Awake()
    {
        InitializeInputActions();
    }

    private void InitializeInputActions()
    {
        if(playerInputs == null)
        playerInputs = GameAssets.Instance.player.GetComponent<PlayerInputs>();

        if(playerMovement == null)
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
        if (InteractionManager.Instance.GetLastIdInteracted() == objective.targetID)
        {
            QuestManager.Instance.CompleteCurrentStep();
        }
        else
        {
            //Debug.LogError($"Esperava id[{objective.targetID}] e o ultimo foi [{InteractionManager.Instance.GetLastIdInteracted()}]");
        }
    }

    private void CheckButtonPressObjective(QuestObjective objective)
    {
        if (IsMenuBlockingTutorial()) return;

        switch (objective.requiredInput)
        {
            case InputsPossibilities.Forward:
                if(playerMovement.forwardPressed) QuestManager.Instance.CompleteCurrentStep();
                break;
            case InputsPossibilities.Backward:
                if (playerMovement.backwardPressed) QuestManager.Instance.CompleteCurrentStep();
                break;
            case InputsPossibilities.Left:
                if (playerMovement.leftPressed) QuestManager.Instance.CompleteCurrentStep();
                break;
            case InputsPossibilities.Right:
                if (playerMovement.rightPressed) QuestManager.Instance.CompleteCurrentStep();
                break;
            case InputsPossibilities.Map:
                break;
            case InputsPossibilities.Recipe:
                break;
            case InputsPossibilities.Pause:
                break;
            case InputsPossibilities.Inventory:
                break;
            case InputsPossibilities.Interact:
                break;
            default:
                break;
        }
    }

    private bool IsMenuBlockingTutorial()
    {
        // Usa sua refer�ncia de UIManager para verificar se menus est�o abertos
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
    // M�todo auxiliar para obter o nome de exibi��o do bot�o
    public string GetButtonDisplayName(string actionName)
    {

        switch (GameAssets.Instance.currentInputType)
        {
            case CurrentInputType.PC:
                return ConvertPCDisplayName(actionName);

            case CurrentInputType.XBOX:
                return ConvertXboxDisplayName(actionName);

            case CurrentInputType.PLAYSTATION:
                return ConvertPlaystationDisplayName(actionName);

            default:
                return actionName.ToUpper();
        }
    }

    private string ConvertPCDisplayName(string unityName)
    {
        switch (unityName)
        {
            case "Space": return "SPACE";
            case "LeftButton": return "MOUSE ESQ";
            case "RightButton": return "MOUSE DIR";
            case "W": return "W";
            case "A": return "A";
            case "S": return "S";
            case "D": return "D";
            default: return unityName.ToUpper();
        }
    }

    private string ConvertXboxDisplayName(string unityName)
    {
        switch (unityName)
        {
            case "Button South": return "A";
            case "Button East": return "B";
            case "Button North": return "Y";
            case "Button West": return "X";
            case "Left Trigger": return "LT";
            case "Right Trigger": return "RT";
            case "Left Shoulder": return "LB";
            case "Right Shoulder": return "RB";
            default: return unityName;
        }
    }

    private string ConvertPlaystationDisplayName(string unityName)
    {
        switch (unityName)
        {
            case "Button South": return "CROSS";
            case "Button East": return "CIRCLE";
            case "Button North": return "TRIANGLE";
            case "Button West": return "SQUARE";
            case "Left Trigger": return "L2";
            case "Right Trigger": return "R2";
            case "Left Shoulder": return "L1";
            case "Right Shoulder": return "R1";
            default: return unityName;
        }
    }
}