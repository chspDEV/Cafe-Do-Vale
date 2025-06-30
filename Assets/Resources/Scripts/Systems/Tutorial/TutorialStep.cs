using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;

[Serializable]
public class TutorialStep
{
    public string stepID;
    public string instructionText;
    public TutorialObjective objective;
    public bool isCompleted;
    public bool hasIndicator;

    [ShowIf("hasIndicator", true)]
    public GameObject visualIndicator; 
}