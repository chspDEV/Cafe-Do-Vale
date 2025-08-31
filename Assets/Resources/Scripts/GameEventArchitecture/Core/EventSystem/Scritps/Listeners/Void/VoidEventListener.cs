namespace GameEventArchitecture.Core.EventSystem.Listeners
{
    using GameEventArchitecture.Core.EventSystem.Base;

    public class VoidEventListener : Tcp4.BaseEventListener<Void, VoidGameEvent, UnityVoidEvent> { }
}