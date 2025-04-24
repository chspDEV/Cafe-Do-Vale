using System;
using System.Collections.Generic;

namespace Tcp4.Resources.Scripts.Systems.DayNightCycle
{
    public enum Season { Winter, Spring, Summer, Fall }

    public class CalendarService
    {
        public event Action<int, int, int> OnDayChanged;
        public event Action OnNewSeason;

        private DateTime _currentDate;
        private Season _currentSeason;

        private Dictionary<int, int> _daysInMonth = new Dictionary<int, int>
        {
            { 1, 30 }, { 2, 28 }, { 3, 31 }, { 4, 30 }, { 5, 31 },
            { 6, 30 }, { 7, 31 }, { 8, 31 }, { 9, 30 }, { 10, 31 },
            { 11, 30 }, { 12, 31 }
        };

        public CalendarService(DateTime startDate)
        {
            _currentDate = startDate;
            _currentSeason = CalculateSeason(startDate.Month);
        }

        public DateTime CurrentDate => _currentDate;
        public Season CurrentSeason => _currentSeason;

        public void AdvanceDay()
        {
            _currentDate = _currentDate.AddDays(1);

            if (_currentDate.Day == 1)
            {
                _currentSeason = CalculateSeason(_currentDate.Month);
                OnNewSeason?.Invoke();
            }

            OnDayChanged?.Invoke(_currentDate.Day, _currentDate.Month, _currentDate.Year);
        }

        private Season CalculateSeason(int month)
        {
            return month switch
            {
                12 or 1 or 2 => Season.Winter,
                3 or 4 or 5 => Season.Spring,
                6 or 7 or 8 => Season.Summer,
                _ => Season.Fall,
            };
        }
    }


}