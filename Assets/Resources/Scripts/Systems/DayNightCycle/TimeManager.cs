using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace Tcp4.Resources.Scripts.Systems.DayNightCycle
{
    public class TimeManager : MonoBehaviour
    {
        [Header("configurables")]
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TimeSettings timeSettings;
        [FormerlySerializedAs("isBrazilianTimeFormat")][SerializeField] private bool isBrazilianFormat = true;

        [Header("Day and Night")]
        [SerializeField] private Light sun;
        [SerializeField] private Light moon;
        [SerializeField] private AnimationCurve lightIntensityCurve;
        [SerializeField] private float maxSunIntensity = 1;
        [SerializeField] private float maxMoonIntensity = 0.5f;
        [SerializeField] private Color dayAmbientLight;
        [SerializeField] private Color nightAmbientLight;
        [SerializeField] private Volume volume;
        [SerializeField] private Material skyboxMaterial;

        private ColorAdjustments _colorAdjustments;

        public TimeService _timeService;
        private CalendarService _calendarService;

        public event Action OnSunrise
        {
            add => _timeService.OnSunrise += value;
            remove => _timeService.OnSunrise -= value;
        }
        public event Action OnSunset
        {
            add => _timeService.OnSunset += value;
            remove => _timeService.OnSunset -= value;
        }
        public event Action OnHourChange
        {
            add => _timeService.OnHourChange += value;
            remove => _timeService.OnHourChange -= value;
        }

        public event Action OnCloseCoffeeShop
        {
            add => _timeService.OnCloseCoffeeShop += value;
            remove => _timeService.OnCloseCoffeeShop -= value;
        }
        public event Action OnOpenCoffeeShop
        {
            add => _timeService.OnOpenCoffeeShop += value;
            remove => _timeService.OnOpenCoffeeShop -= value;
        }

        private void Start()
        {
            _timeService = new TimeService(timeSettings);
            DateTime startDate = new DateTime(
                timeSettings.startYear,
                timeSettings.startMonth,
                timeSettings.startDay,
                (int)timeSettings.startHour,
                0,
                0
            );
            _calendarService = new CalendarService(startDate);
            _timeService.OnDayPassed += () => _calendarService.AdvanceDay();


            volume.profile.TryGet(out _colorAdjustments);
            OnSunrise += () => Debug.Log("Sunrise");
            OnSunset += () => Debug.Log("Sunset");
            OnOpenCoffeeShop += () => Debug.Log("Open coffee shop");
            OnCloseCoffeeShop += () => Debug.Log("Close coffee shop");

            //OnHourChange += () => Debug.Log("Hour change");
            _calendarService.OnDayChanged += (day, month, year) =>
            {
                Debug.Log($"Novo dia: {day}/{month}/{year}");
            };
            _calendarService.OnNewSeason += () =>
            {
                Debug.Log($"Nova estação: {_calendarService.CurrentSeason}");
            };
        }

        private void Update()
        {
            UpdateTimeOfDay();
            RotateSun();
            UpdateLightSettings();
            UpdateSkyBlend();
        }


        void UpdateSkyBlend()
        {
            float dotProduct = Vector3.Dot(sun.transform.forward, Vector3.up);
            float blend = Mathf.Lerp(0, 1, lightIntensityCurve.Evaluate(dotProduct));
            skyboxMaterial.SetFloat("_Blend", blend);
        }
        void UpdateLightSettings()
        {
            float dotProduct = Vector3.Dot(sun.transform.forward, Vector3.down);
            sun.intensity = Mathf.Lerp(0, maxSunIntensity, lightIntensityCurve.Evaluate(dotProduct));
            moon.intensity = Mathf.Lerp(maxMoonIntensity, 0, lightIntensityCurve.Evaluate(dotProduct));
            if (_colorAdjustments == null) return;
            _colorAdjustments.colorFilter.value = Color.Lerp(nightAmbientLight, dayAmbientLight,
                lightIntensityCurve.Evaluate(dotProduct));
        }
        void RotateSun()
        {
            float rotation = _timeService.CalculateSunAngle();
            sun.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.right);
        }
        void UpdateTimeOfDay()
        {
            _timeService.UpdateTime(Time.deltaTime);

            if (timeText != null)
            {
                string dateFormat = "dd/MM/yyyy";
                string timeFormat = isBrazilianFormat ? "HH:mm" : "hh:mm tt";
                timeText.text = _timeService.CurrentTime.ToString($"{dateFormat} / {timeFormat}");
            }
        }
        public void AdvanceTime(float hours)
        {
            try
            {
                _timeService.AdvanceTime(hours);
                Debug.Log($"Avançou {hours} horas.");
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.LogError(e.Message);
            }
        }
        public void Sleep()
        {
            _timeService.SleepUntilMorning();
            Debug.Log("Dormindo até o amanhecer...");
        }

    }
}
