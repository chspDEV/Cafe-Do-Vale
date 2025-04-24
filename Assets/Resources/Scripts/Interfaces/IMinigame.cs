using UnityEngine;

namespace Tcp4
{
    public interface IMinigame
    {
        void InitializeMiniGame();
        void StartMiniGame();
        void UpdateMiniGame();
        void EndMiniGame();
        bool IsCompleted();
    }
}
