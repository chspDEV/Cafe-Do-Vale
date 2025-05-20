using UnityEngine;
using Tcp4.Assets.Resources.Scripts.Managers;  // onde está seu TimeManager
using Sirenix.OdinInspector;

namespace Tcp4
{
    [RequireComponent(typeof(Collider))]
    public class CollectableSettings : MonoBehaviour
    {
        [FoldoutGroup("Config")]
        [LabelText("Apenas à Noite")]
        [SerializeField] private bool onlyNight;

        private void Awake()
        {
            UpdateActiveState(TimeManager.Instance.isDay);
        }

        private void OnEnable()
        {
            TimeManager.Instance.OnDayNightChanged += UpdateActiveState;
        }

        private void OnDisable()
        {
            TimeManager.Instance.OnDayNightChanged -= UpdateActiveState;
        }

        /// <summary>
        /// Chamado sempre que o TimeManager alterna entre dia e noite.
        /// </summary>
        private void UpdateActiveState(bool isDay)
        {
            // se onlyNight==true -> ativo somente quando !isDay
            bool shouldBeActive = onlyNight ? !isDay : isDay;
            if (gameObject.activeSelf != shouldBeActive)
                gameObject.SetActive(shouldBeActive);
        }
    }
}
