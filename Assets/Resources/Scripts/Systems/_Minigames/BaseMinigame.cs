using System;
using UnityEngine;

public abstract class BaseMinigame : MonoBehaviour
{
    public event Action<MinigameResult> OnMinigameConcluded;

    public abstract void StartMinigame();

    protected void ConcludeMinigame(MinigamePerformance performance)
    {
        MinigameResult result = new MinigameResult { performance = performance };

        OnMinigameConcluded?.Invoke(result);
    }
}