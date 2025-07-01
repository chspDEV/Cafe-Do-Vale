using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;

[Serializable]
public class QuestStep
{
    public string stepID;
    public string instructionText;
    public QuestObjective objective;
    public bool isCompleted;
    public bool hasIndicator;

    [ShowIf("hasIndicator", true)]
    public GameObject visualIndicator; 
}