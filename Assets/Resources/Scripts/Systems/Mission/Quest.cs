using System.Collections.Generic;
using System;
using UnityEngine;


[CreateAssetMenu(fileName = "Quest", menuName = "ScriptableObjects/Quest", order = 1)]
public class Quest: ScriptableObject
{
    public string questID;
    public string questName;
    public List<QuestStep> steps;
    public bool isCompleted;
    public bool isStarted;
}
