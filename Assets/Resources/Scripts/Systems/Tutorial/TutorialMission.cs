using System.Collections.Generic;
using System;

[Serializable]
public class TutorialMission
{
    public string missionID;
    public string missionName;
    public List<TutorialStep> steps;
    public bool isCompleted;
}
