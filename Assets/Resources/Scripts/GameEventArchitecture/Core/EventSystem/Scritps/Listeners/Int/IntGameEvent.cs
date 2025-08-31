namespace GameEventArchitecture.Core.EventSystem.Listeners
{
    using GameEventArchitecture.Core.EventSystem.Base;
    using UnityEngine;


    [CreateAssetMenu(menuName = "Game Architecture/Game Events/Int Event")]
    public class IntGameEvent : BaseGameEvent<int> { }
}