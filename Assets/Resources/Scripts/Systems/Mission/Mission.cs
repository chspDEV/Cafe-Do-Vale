using UnityEngine;

public abstract class Mission : ScriptableObject
{
    public string missionName;
    [TextArea] public string missionDescription;
    public Sprite missionIcon;

    public abstract bool CheckCompletion();
    public abstract void OnComplete();
}