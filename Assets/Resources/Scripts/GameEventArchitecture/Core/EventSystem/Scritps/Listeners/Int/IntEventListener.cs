namespace GameEventArchitecture.Core.EventSystem.Listeners
{
    using GameEventArchitecture.Core.EventSystem.Base;

    public class IntEventListener : Tcp4.BaseEventListener<int, IntGameEvent, UnityIntEvent> { }
}