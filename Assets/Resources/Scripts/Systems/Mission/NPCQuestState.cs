using System.Collections.Generic;

/// <summary>
/// Classe para gerenciar e persistir o estado dos NPCs com quests
/// </summary>
[System.Serializable]
public class NPCQuestState
{
    public string npcID;
    public int currentMissionIndex;
    public bool hasStartedCurrentMission;
    public string lastStartedMissionID;
    public List<string> completedDialogues = new List<string>();

    public NPCQuestState(string id)
    {
        npcID = id;
        currentMissionIndex = -1;
        hasStartedCurrentMission = false;
        lastStartedMissionID = "";
    }
}
