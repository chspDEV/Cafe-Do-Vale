namespace GameEventArchitecture.Core.EventSystem.Listeners
{
    using Tcp4.Assets.Resources.Scripts.Systems.Almanaque;
    using UnityEngine;
    using UnityEngine.Events;
    [System.Serializable] public class UnityAlmanaqueEvent : UnityEvent<BaseAlmanaque> { }
}
