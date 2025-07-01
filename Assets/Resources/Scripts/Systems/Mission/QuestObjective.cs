using Sirenix.OdinInspector;
using UnityEngine;

public enum QuestObjectiveType
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
public class QuestObjective
{
    [HorizontalGroup("Type"), LabelWidth(100)]
    public QuestObjectiveType objectiveType = QuestObjectiveType.None;

    // Campo condicional para botões
    [ShowIf("objectiveType", QuestObjectiveType.PressButton)]
    [HorizontalGroup("Type"), LabelWidth(100)]
    public InputsPossibilities requiredInput;

    // Campos condicionais para localização
    [ShowIf("objectiveType", QuestObjectiveType.ReachLocation)]
    [HorizontalGroup("Location"), LabelWidth(120)]
    public Vector3 targetPosition;

    [Space(5)]
    [Range(0.1f,20)]
    [ShowIf("objectiveType", QuestObjectiveType.ReachLocation)]
    public float radius = 2f;

    // Campo condicional para itens e interações
    [ShowIf("@objectiveType == QuestObjectiveType.CollectItem || " +
             "objectiveType == QuestObjectiveType.InteractWithObject")]
    [HorizontalGroup("Target"), LabelWidth(100)]
    public string targetID;

    // Métodos auxiliares para Odin
    private bool ShouldShowLocationFields() => objectiveType == QuestObjectiveType.ReachLocation;
    private bool ShouldShowTargetID() =>
        objectiveType == QuestObjectiveType.CollectItem ||
        objectiveType == QuestObjectiveType.InteractWithObject;
}