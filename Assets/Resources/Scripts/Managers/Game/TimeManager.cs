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

        [FoldoutGroup("Eventos", expanded: false)]
        [SerializeField] private bool showEvents = true;

        [FoldoutGroup("Eventos")]
        public event Action<float> OnTimeChanged;

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
        private bool isDay;

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

        protected override void Awake()
        {
            base.Awake();
            gameDate = new DateTime(2024, 1, 1);
            CurrentHour = startHour;
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
                CurrentHour -= 24;
                gameDate = gameDate.AddDays(1);
            }

            OnTimeChanged?.Invoke(CurrentHour);

            if (Mathf.FloorToInt(CurrentHour) == (int)startHour && !isDay)
            {
                OnOpenCoffeeShop?.Invoke();
                isDay = true;
            }
            else if (Mathf.FloorToInt(CurrentHour) == (int)closeHour && isDay)
            {
                OnCloseCoffeeShop?.Invoke();
                isDay = false;
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

        // Restante dos métodos permanecem inalterados
        private void UpdateLighting()
        {
            if (directionalLight == null) return;

            float timePercent = CurrentHour / 24f;

            float sunRotation = Mathf.Lerp(-90, 270, timePercent);
            directionalLight.transform.rotation = Quaternion.Euler(sunRotation, -150f, 0);

            directionalLight.color = lightColor.Evaluate(timePercent);
            directionalLight.intensity = lightIntensity.Evaluate(timePercent);

            RenderSettings.fogDensity = Mathf.Lerp(nightFogRange.y, dayFogRange.x, timePercent);

            UpdatePostProcessing(timePercent);
        }

        private void UpdatePostProcessing(float timePercent)
        {
            if (PostProcessManager.Instance != null)
            {
                Color temperatureColor = Color.Lerp(nightColor, dayColor, timePercent);
                PostProcessManager.Instance.SetTemperature(temperatureColor);
                PostProcessManager.Instance.SetExposure(Mathf.Lerp(0.3f, 1.2f, timePercent));
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