namespace GameEventArchitecture.Core.EventSystem.Listeners
{
    using GameEventArchitecture.Core.EventSystem.Base;
    using UnityEngine;


    [CreateAssetMenu(menuName = "Game Architecture/Game Events/Float Event")]
    public class FloatGameEvent : BaseGameEvent<float> { }
}