using Sirenix.OdinInspector;
using UnityEngine;

public enum TutorialObjectiveType
{
    None,
    PressButton,
    ReachLocation,
    CollectItem,
    InteractWithObject,
    InfoOnly
}

public enum InputsPossibilities
{
    Forward,
    Backward,
    Left,
    Right,
    Map,
    Recipe,
    Pause,
    Inventory,
    Interact
}

[System.Serializable]
public class TutorialObjective
{
    [HorizontalGroup("Type"), LabelWidth(100)]
    public TutorialObjectiveType objectiveType = TutorialObjectiveType.None;

    // Campo condicional para botões
    [ShowIf("objectiveType", TutorialObjectiveType.PressButton)]
    [HorizontalGroup("Type"), LabelWidth(100)]
    public InputsPossibilities requiredInput;

    // Campos condicionais para localização
    [ShowIf("objectiveType", TutorialObjectiveType.ReachLocation)]
    [HorizontalGroup("Location"), LabelWidth(120)]
    public Vector3 targetPosition;

    [ShowIf("objectiveType", TutorialObjectiveType.ReachLocation)]
    [HorizontalGroup("Location"), LabelWidth(140), Range(0.1f, 10f)]
    public float completionRadius = 2f;

    // Campo condicional para itens e interações
    [ShowIf("@objectiveType == TutorialObjectiveType.CollectItem || " +
             "objectiveType == TutorialObjectiveType.InteractWithObject")]
    [HorizontalGroup("Target"), LabelWidth(100)]
    public string targetID;

    // Métodos auxiliares para Odin
    private bool ShouldShowLocationFields() => objectiveType == TutorialObjectiveType.ReachLocation;
    private bool ShouldShowTargetID() =>
        objectiveType == TutorialObjectiveType.CollectItem ||
        objectiveType == TutorialObjectiveType.InteractWithObject;
}