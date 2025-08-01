using UnityEngine;

public class MinigameTrigger : MonoBehaviour
{

    public MinigameData minigameToStart;

    public void TriggerMinigame()
    {
        if (this.minigameToStart != null)
        {
            MinigameManager.Instance.StartMinigame(this.minigameToStart);
        }
    }
}