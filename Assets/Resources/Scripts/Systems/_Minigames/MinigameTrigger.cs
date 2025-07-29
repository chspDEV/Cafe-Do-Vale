using UnityEngine;

public class MinigameTrigger : MonoBehaviour
{

    public MinigameData minigameToStart;

    public void TriggerMinigame()
    {
        if (minigameToStart != null)
        {
            MinigameManager.Instance.StartMinigame(minigameToStart);
        }
    }
}