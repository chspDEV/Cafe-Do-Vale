namespace GameEventArchitecture.Core.EventSystem.Listeners
{
    using GameEventArchitecture.Core.EventSystem.Base;

    public class StringEventListener : Tcp4.BaseEventListener<string, StringGameEvent, UnityStringEvent> { }
}