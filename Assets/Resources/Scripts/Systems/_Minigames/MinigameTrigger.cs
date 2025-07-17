using UnityEngine;

public class MinigameTrigger : MonoBehaviour
{

    //arraste o asset do minigame data aqui no inspector
    public MinigameData minigameToStart;

    //voce pode chamar este metodo a partir de um evento de clique de botao,
    //de uma colisao, ou de um dialogo com npc.
    public void TriggerMinigame()
    {
        if (minigameToStart != null)
        {
            MinigameManager.Instance.StartMinigame(minigameToStart);
        }
    }
}