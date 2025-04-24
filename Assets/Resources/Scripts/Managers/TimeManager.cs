using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace Tcp4.Assets.Resources.Scripts.Managers
{
    public class TimeManager : Singleton<TimeManager>
    {
        [TitleGroup("Configurações de Tempo")]
        [PropertyRange(1, 100), Tooltip("Multiplicador de velocidade do tempo")]
        [SerializeField] private float timeMultiplier = 1f;

        [TitleGroup("Horário Comercial")]
        [PropertyRange(0, 24), SuffixLabel("horas", true)]
        [SerializeField] private float startHour = 9f;

        [PropertyRange(0, 24), SuffixLabel("horas", true)]
        [SerializeField] private float closeHour = 18f;

        [TitleGroup("Iluminação Diurna")]
        [Required("Selecione a luz direcional principal")]
        [SerializeField] private Light directionalLight;

        [Title("Gradiente de Cores")]
        [ColorUsage(true, true), PreviewField(60)]
        [SerializeField] private Gradient lightColor;

        [Title("Intensidade da Luz")]
        [HorizontalGroup("LightSettings"), PreviewField(60)]
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

        [ShowInInspector, ReadOnly, ProgressBar(0, 24)]
        public float CurrentHour { get; private set; }

        [Button("Validar Referência de Luz")]
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
            currentTime = DateTime.Now.Date + TimeSpan.FromHours(startHour);
            CurrentHour = startHour;
        }

        private void Update()
        {
            UpdateTime();
            UpdateLighting();
        }

        private void UpdateTime()
        {
            // Atualiza o tempo do jogo
            CurrentHour += Time.deltaTime * timeMultiplier / 3600;
            
            if(CurrentHour >= 24)
            {
                CurrentHour -= 24;
            }

            // Dispara eventos de hora em hora
            OnTimeChanged?.Invoke(CurrentHour);

            // Verifica abertura/fechamento
            if(Mathf.FloorToInt(CurrentHour) == (int)startHour && !isDay)
            {
                OnOpenCoffeeShop?.Invoke();
                isDay = true;
            }
            else if(Mathf.FloorToInt(CurrentHour) == (int)closeHour && isDay)
            {
                OnCloseCoffeeShop?.Invoke();
                isDay = false;
            }
        }

        private void UpdateLighting()
        {
            if(directionalLight == null) return;

            // Calcula a porcentagem do ciclo dia/noite
            float timePercent = CurrentHour / 24f;
            
            // Atualiza rotação da luz
            float sunRotation = Mathf.Lerp(-90, 270, timePercent);
            directionalLight.transform.rotation = Quaternion.Euler(sunRotation, -150f, 0);

            // Atualiza cor e intensidade
            directionalLight.color = lightColor.Evaluate(timePercent);
            directionalLight.intensity = lightIntensity.Evaluate(timePercent);

            // Atualiza neblina
            RenderSettings.fogDensity = Mathf.Lerp(nightFogRange.y, dayFogRange.x, timePercent);
            
            // Atualiza pós-processamento
            UpdatePostProcessing(timePercent);
        }

        private void UpdatePostProcessing(float timePercent)
        {
            // Ajusta exposição e temperatura de cor
            if(PostProcessManager.instance != null)
            {
                PostProcessManager.instance.SetExposure(Mathf.Lerp(0.3f, 1.2f, timePercent));
                PostProcessManager.instance.SetTemperature(Mathf.Lerp(70f, 20f, timePercent));
            }
        }

        public void SetTimeSpeed(float multiplier)
        {
            timeMultiplier = multiplier;
        }

        public string GetFormattedTime()
        {
            TimeSpan time = TimeSpan.FromHours(CurrentHour);
            return time.ToString("hh':'mm");
        }
    

    }
}