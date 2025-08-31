namespace GameEventArchitecture.Core.EventSystem.Listeners
{
    using GameEventArchitecture.Core.EventSystem.Base;
    using UnityEngine;


    [CreateAssetMenu(menuName = "Game Architecture/Game Events/String Event")]
    public class StringGameEvent : BaseGameEvent<string> { }
}