using System;
using UnityEngine;

public abstract class BaseMinigame : MonoBehaviour
{
    //evento que o manager vai ouvir para saber quando o minigame acabou
    public event Action<MinigameResult> OnMinigameConcluded;

    //metodo para ser chamado e iniciar a logica do minigame
    public abstract void StartMinigame();

    //metodo auxiliar para as classes filhas chamarem quando terminarem
    protected void ConcludeMinigame(MinigamePerformance performance)
    {
        //cria o resultado
        MinigameResult result = new MinigameResult { performance = performance };

        //dispara o evento para notificar o manager
        OnMinigameConcluded?.Invoke(result);
    }
}