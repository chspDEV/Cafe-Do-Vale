using GameEventArchitecture.Core.EventSystem.Listeners;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue Data")]
public class DialogueData : SerializedScriptableObject // Use SerializedScriptableObject para Odin
{
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
}

// -------------------------------------------------------------------------------------
// CLASSE PRINCIPAL PARA AS LINHAS DE DIÁLOGO
// -------------------------------------------------------------------------------------

[System.Serializable]
public class DialogueLine
{
    [HorizontalGroup("LineData", 120), HideLabel]
    [PreviewField(100, ObjectFieldAlignment.Left)]
    public Sprite characterAvatar;

    [VerticalGroup("LineData/Right")]
    public string characterName;

    [VerticalGroup("LineData/Right")]
    [TextArea(3, 5)]
    public string text;

    [BoxGroup("Game Events")]
    [ToggleLeft]
    [LabelText("Disparar eventos ao exibir esta linha?")]
    public bool triggerEvents;

    // Esta lista só aparecerá no Inspector se 'triggerEvents' for marcado.
    [ShowIf("triggerEvents")]
    [ListDrawerSettings(AddCopiesLastElement = true)]
    [SerializeReference] // Atributo essencial para suportar polimorfismo na serialização do Unity.
    [BoxGroup("Game Events")]
    public List<DialogueEventAction> eventActions = new List<DialogueEventAction>();
}


// -------------------------------------------------------------------------------------
// ARQUITETURA DE EVENTOS 
// -------------------------------------------------------------------------------------

/// <summary>
/// Classe base abstrata para todas as ações de evento que podem ser disparadas pelo diálogo.
/// </summary>
public abstract class DialogueEventAction
{
    public abstract void Trigger();
}

// --- IMPLEMENTAÇÕES CONCRETAS ---

[System.Serializable]
public class VoidEventAction : DialogueEventAction
{
    [InlineProperty, HideLabel, Tooltip("O VoidGameEvent a ser disparado.")]
    public VoidGameEvent eventToRaise;

    public override void Trigger()
    {
        if (eventToRaise != null)
        {
            eventToRaise.Broadcast(new Void());
        }
    }
}

[System.Serializable]
public class IntEventAction : DialogueEventAction
{
    [HorizontalGroup("Event", Width = 0.7f), HideLabel, Tooltip("O IntGameEvent a ser disparado.")]
    public IntGameEvent eventToRaise;

    [HorizontalGroup("Event"), HideLabel, Tooltip("O valor inteiro a ser enviado.")]
    public int valueToBroadcast;

    public override void Trigger()
    {
        if (eventToRaise != null)
        {
            eventToRaise.Broadcast(valueToBroadcast);
        }
    }
}
