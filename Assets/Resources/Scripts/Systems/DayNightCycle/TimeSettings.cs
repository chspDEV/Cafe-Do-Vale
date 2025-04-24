using UnityEngine;

namespace Tcp4.Resources.Scripts.Systems.DayNightCycle
{
    [CreateAssetMenu(fileName = "TimeSettings", menuName = "DayNightCycle/TimeSettings", order = 0)]
    public class TimeSettings : ScriptableObject
    {
        [Header("Hours")]
        public short timeMultiplier = 2000;
        public byte startHour = 12;
        public byte sunriseHour = 6;
        public byte sunsetHour = 18;
        public byte closeCoffeeShop = 00;

        [Header("Calender")]
        public int startDay = 1;
        public int startMonth = 1;
        public int startYear = 2023;
    }
}
