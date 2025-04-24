using UnityEngine;

namespace Tcp4
{
    public abstract class MinigameBase : MonoBehaviour, IMinigame
    {
       public abstract void InitializeMiniGame();
       public abstract void StartMiniGame();
       public abstract void UpdateMiniGame();
       public abstract void EndMiniGame();
       public abstract bool IsCompleted();
    }
}
