using UnityEngine;

namespace Tcp4
{
    //o estado atual do npc, o que ele esta fazendo/onde esta
    public enum ClientState
    {
        WalkingOnStreet,
        GoingToQueue,
        InQueue,
        GoingToCounter,
        AtCounter,
        WaitingForOrder,
        GoingToSeat,
        Seated,
        LeavingShop
    }

    //a acao que o manager deve executar na thread principal
    public enum ClientAction
    {
        None,
        MoveToTarget,
        ShowOrderBubble,
        GiveReward,
        ApplyPenalty,
        WaitOrder,
        Deactivate
    }

}
