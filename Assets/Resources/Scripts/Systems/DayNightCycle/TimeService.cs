using System;
using Tcp4.Resources.Scripts.Systems.Utility;
using UnityEngine;

namespace Tcp4.Resources.Scripts.Systems.DayNightCycle
{
    public class TimeService 
    {
        readonly TimeSettings settings;
        DateTime currentTime;
        readonly TimeSpan sunriseTime;
        readonly TimeSpan sunsetTime;
        readonly TimeSpan closeCoffeeShop;

        public event Action OnSunrise = delegate { }; 
        public event Action OnSunset = delegate { }; 

        public event Action OnCloseCoffeeShop = delegate { };
        public event Action OnOpenCoffeeShop = delegate { };

        public event Action OnHourChange = delegate { }; 
        public event Action OnDayPassed = delegate { };
        
        private readonly Observable<bool> isDayTime;
        private readonly Observable<bool> isOpenShop;
        private readonly Observable<int> currentHour;
        
        public TimeService(TimeSettings settings)
        {
            this.settings = settings;
            currentTime = new DateTime(settings.startYear, settings.startMonth, settings.startDay) 
                          + TimeSpan.FromHours(settings.startHour);
            sunriseTime = TimeSpan.FromHours(settings.sunriseHour);
            sunsetTime = TimeSpan.FromHours(settings.sunsetHour);
            closeCoffeeShop = TimeSpan.FromHours(settings.closeCoffeeShop);

            isDayTime = new Observable<bool>(IsDayTime());
            isOpenShop = new Observable<bool>(IsOpenShop());
            currentHour = new Observable<int>(currentTime.Hour);
            
            isDayTime.ValueChanged += day => (day ? OnSunrise : OnSunset)?.Invoke();
            isOpenShop.ValueChanged += open => (open ? OnOpenCoffeeShop : OnCloseCoffeeShop)?.Invoke();
            currentHour.ValueChanged += _ => OnHourChange?.Invoke();
        }

        public void UpdateTime(float deltaTime)
        {
            DateTime previousTime = currentTime;
            currentTime = currentTime.AddSeconds(deltaTime * settings.timeMultiplier);
            isDayTime.Value = IsDayTime();
            isOpenShop.Value = IsOpenShop();
            currentHour.Value = currentTime.Hour;
            if (previousTime.Day != currentTime.Day) OnDayPassed?.Invoke();
        }
        
        public float CalculateSunAngle()
        {
            bool isDay = IsDayTime();
            float startDegree = isDay ? 0 : 180;
            TimeSpan start = isDay ? sunriseTime : sunsetTime;
            TimeSpan end = isDay ? sunsetTime : sunriseTime;

            TimeSpan totalTime = CalculateDifference(start, end);
            TimeSpan elapsedTime = CalculateDifference(start, currentTime.TimeOfDay);
            
            double percentage = elapsedTime.TotalMinutes / totalTime.TotalMinutes;
            return Mathf.Lerp(startDegree, startDegree + 180, (float)percentage);
        }
        
        public void AdvanceTime(float hours)
        {
            if (hours < 0 || hours + currentTime.Hour > 23)
                throw new ArgumentOutOfRangeException("Você pode avançar no máximo até 23:00.");

            currentTime = currentTime.AddHours(hours);
            isDayTime.Value = IsDayTime();
            currentHour.Value = currentTime.Hour;
            
            if (currentTime.Day != (currentTime.AddHours(hours)).Day)
                OnDayPassed?.Invoke();
        }

        public void SleepUntilMorning()
        {
            var nextSunrise = sunriseTime.Add(new TimeSpan(24, 0, 0)); 
            var newDate = currentTime.Date.AddDays(1).Add(sunriseTime);
            currentTime = newDate;
            isDayTime.Value = IsDayTime();
            currentHour.Value = currentTime.Hour;
            OnDayPassed?.Invoke(); 
        }
        
        public DateTime CurrentTime => currentTime;
        bool IsDayTime() => currentTime.TimeOfDay > sunriseTime && currentTime.TimeOfDay < sunsetTime;
        bool IsOpenShop() => currentTime.TimeOfDay >= sunsetTime && currentTime.TimeOfDay != closeCoffeeShop;

        TimeSpan CalculateDifference(TimeSpan from, TimeSpan to)
        {
            TimeSpan difference = to - from;
            return difference.TotalHours < 0 ? difference + TimeSpan.FromHours(24) : difference;
        }
    }
}
