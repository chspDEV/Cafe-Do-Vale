using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace Tcp4.Assets.Resources.Scripts.Managers
{
    public class TimeManager : Singleton<TimeManager>
    {
        [TitleGroup("Configurações de Tempo")]
        [PropertyRange(1, 500)]
        [SerializeField] private float timeMultiplier = 1f;
        [SerializeField] private float initialHour = 6f;
        [SerializeField] public bool isFirstDay = true;

        [TitleGroup("Horário Comercial")]
        [PropertyRange(0, 24)]
        [SerializeField] private float startHour = 9f;

        [PropertyRange(0, 24)]
        [SerializeField] private float closeHour = 18f;

        [TitleGroup("Iluminação Diurna")]
        [Required]
        [SerializeField] private Light directionalLight;

        [Title("Gradiente de Cores")]
        [SerializeField] private Gradient lightColor;

        [Title("Intensidade da Luz")]
        [SerializeField] private AnimationCurve lightIntensity;

        [TitleGroup("Efeitos de Neblina")]
        [MinMaxSlider(0, 0.2f, true)]
        [SerializeField] private Vector2 nightFogRange = new Vector2(0.05f, 0.1f);

        [MinMaxSlider(0, 0.2f, true)]
        [SerializeField] private Vector2 dayFogRange = new Vector2(0.01f, 0.05f);


        [FoldoutGroup("Eventos")]
        public event Action<float> OnTimeChanged;

        [FoldoutGroup("Eventos")]
        public event Action OnResetDay;

        [FoldoutGroup("Eventos")]
        public event Action<bool> OnDayNightChanged;

        [FoldoutGroup("Eventos")]
        public event Action OnOpenCoffeeShop;

        [FoldoutGroup("Eventos")]
        public event Action OnCloseCoffeeShop;

        [TitleGroup("Temperatura de Cor")]
        [ColorUsage(true, true)]
        [SerializeField] private Color nightColor = Color.blue;

        [ColorUsage(true, true)]
        [SerializeField] private Color dayColor = new Color(1f, 0.6f, 0.4f);

        [ShowInInspector, ReadOnly, ProgressBar(0, 24)]
        public float CurrentHour { get; private set; }

        [ShowInInspector, ReadOnly]
        public string CurrentDate => GetFormattedDate();

        private DateTime gameDate;
        public bool isDay;

        [Button("Validar Referência de Luz")]
        [Obsolete]
        private void ValidateLightReference()
        {
            if (directionalLight == null)
            {
                directionalLight = GameObject.FindObjectOfType<Light>();
                Debug.LogWarning("Luz direcional atribuída automaticamente!");
            }
        }

        public override void Awake()
        {
            base.Awake();
            gameDate = new DateTime(2024, 1, 1);
            CurrentHour = initialHour;
            isDay = CurrentHour >= startHour && CurrentHour < closeHour;
        }

        private void Update()
        {
            UpdateTime();
            UpdateLighting();
        }

        private void UpdateTime()
        {
            CurrentHour += Time.deltaTime * (timeMultiplier * 10) / 3600;

            if (CurrentHour >= 24)
            {
                if (isFirstDay) isFirstDay = false;

                CurrentHour -= 24;
                OnResetDay?.Invoke();
                gameDate = gameDate.AddDays(1);
            }

            OnTimeChanged?.Invoke(CurrentHour);

            if (Mathf.FloorToInt(CurrentHour) == (int)startHour && !isDay)
            {
                if (!isFirstDay)
                    OnOpenCoffeeShop?.Invoke();

                isDay = true;
                OnDayNightChanged?.Invoke(isDay);
            }
            else if (Mathf.FloorToInt(CurrentHour) == (int)closeHour && isDay)
            {
                if (!isFirstDay)
                    OnCloseCoffeeShop?.Invoke();

                isDay = false;
                OnDayNightChanged?.Invoke(isDay);
            }
        }

        public string GetFormattedDate()
        {
            return gameDate.ToString("dd/MM/yyyy");
        }

        public string GetFullDateTime()
        {
            return $"{GetFormattedDate()} {GetFormattedTime(CurrentHour)}";
        }

        private void UpdateLighting()
        {
            if (directionalLight == null) return;

            float timePercent = CurrentHour / 24f;

            float sunRotation = Mathf.Lerp(-90, 270, timePercent);
            directionalLight.transform.rotation = Quaternion.Euler(sunRotation, -150f, 0);

            directionalLight.color = lightColor.Evaluate(timePercent);
            directionalLight.intensity = lightIntensity.Evaluate(timePercent);

            UpdatePostProcessing(timePercent);
            UpdateFog(timePercent);
        }

        private void UpdateFog(float timePercent)
        {
            float dayNightLerpFactor = 1f - Mathf.Abs(timePercent - 0.5f) * 2f;
            dayNightLerpFactor = Mathf.Clamp01(dayNightLerpFactor);

            float targetFogDensity = Mathf.Lerp(nightFogRange.y, dayFogRange.x, dayNightLerpFactor);
            RenderSettings.fogDensity = targetFogDensity;
        }

        private void UpdatePostProcessing(float timePercent)
        {
            if (PostProcessManager.Instance != null)
            {
                float dayNightLerpFactor = 1f - Mathf.Abs(timePercent - 0.5f) * 2f;
                dayNightLerpFactor = Mathf.Clamp01(dayNightLerpFactor);

                Color temperatureColor = Color.Lerp(nightColor, dayColor, dayNightLerpFactor);
                PostProcessManager.Instance.SetTemperature(temperatureColor);

                float exposure = Mathf.Lerp(0.3f, 1.2f, dayNightLerpFactor);
                PostProcessManager.Instance.SetExposure(exposure);
            }
            else
            {
                Debug.LogError("PostProcessManager nao encontrado!");
            }
        }

        public void SetTimeSpeed(float multiplier)
        {
            timeMultiplier = multiplier;
        }

        public string GetFormattedTime(float hour)
        {
            TimeSpan time = TimeSpan.FromHours(hour);
            return time.ToString("hh':'mm");
        }
    }
}