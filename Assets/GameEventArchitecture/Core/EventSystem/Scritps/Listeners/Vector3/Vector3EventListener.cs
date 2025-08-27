namespace GameEventArchitecture.Core.EventSystem.Listeners
{
    using GameEventArchitecture.Core.EventSystem.Base;
    using UnityEngine;

    public class Vector3EventListener : BaseEventListener<Vector3, Vector3GameEvent, UnityVector3Event> { }
}