namespace GameEventArchitecture.Core.EventSystem.Listeners
{
    using GameEventArchitecture.Core.EventSystem.Base;

    public class FloatEventListener : Tcp4.BaseEventListener<float, FloatGameEvent, UnityFloatEvent> { }
}