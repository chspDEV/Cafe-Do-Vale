using UnityEngine;
using System.Collections;

namespace Tcp4
{
    /// <summary>
    /// Componente para gerenciar indicadores visuais de quest (como exclama��es, anima��es, etc.)
    /// </summary>
    public class QuestVisualIndicator : MonoBehaviour
    {
        [Header("Indicator Settings")]
        [SerializeField] private IndicatorType indicatorType = IndicatorType.Simple;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.3f;
        [SerializeField] private bool rotateIndicator = true;
        [SerializeField] private float rotationSpeed = 90f;

        [Header("Animation Settings")]
        [SerializeField] private AnimationCurve scaleAnimation = AnimationCurve.EaseInOut(0, 1, 1, 1.2f);
        [SerializeField] private float animationDuration = 1f;
        [SerializeField] private bool loopAnimation = true;

        [Header("Visual Elements")]
        [SerializeField] public GameObject[] visualElements; // Elementos que ser�o animados
        [SerializeField] private ParticleSystem[] particles; // Part�culas opcionais

        private Vector3 startPosition;
        private Vector3[] originalScales;
        private bool isActive = false;
        private Coroutine animationCoroutine;

        public enum IndicatorType
        {
            Simple,        // Apenas liga/desliga
            Bobbing,       // Faz movimento de subir/descer
            Scaling,       // Anima��o de escala
            Rotating,      // Rota��o constante
            Pulsing        // Combina��o de escala e transpar�ncia
        }

        private void Awake()
        {
            startPosition = transform.localPosition;

            // Armazena escalas originais dos elementos visuais
            if (visualElements != null && visualElements.Length > 0)
            {
                originalScales = new Vector3[visualElements.Length];
                for (int i = 0; i < visualElements.Length; i++)
                {
                    if (visualElements[i] != null)
                        originalScales[i] = visualElements[i].transform.localScale;
                }
            }

            // Inicia desativado
            SetActive(false);
        }

        /// <summary>
        /// Ativa ou desativa o indicador
        /// </summary>
        public void SetActive(bool active)
        {
            if (isActive == active) return;

            isActive = active;

            if (active)
            {
                gameObject.SetActive(true);
                StartIndicatorAnimation();

                // Ativa part�culas se houver
                if (particles != null)
                {
                    foreach (var particle in particles)
                    {
                        if (particle != null)
                            particle.Play();
                    }
                }
            }
            else
            {
                StopIndicatorAnimation();

                // Para part�culas
                if (particles != null)
                {
                    foreach (var particle in particles)
                    {
                        if (particle != null)
                            particle.Stop();
                    }
                }

                gameObject.SetActive(false);
            }
        }

        private void StartIndicatorAnimation()
        {
            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);

            switch (indicatorType)
            {
                case IndicatorType.Simple:
                    // N�o faz anima��o, apenas mostra
                    break;

                case IndicatorType.Bobbing:
                    animationCoroutine = StartCoroutine(BobbingAnimation());
                    break;

                case IndicatorType.Scaling:
                    animationCoroutine = StartCoroutine(ScalingAnimation());
                    break;

                case IndicatorType.Rotating:
                    animationCoroutine = StartCoroutine(RotatingAnimation());
                    break;

                case IndicatorType.Pulsing:
                    animationCoroutine = StartCoroutine(PulsingAnimation());
                    break;
            }
        }

        private void StopIndicatorAnimation()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }

            // Reseta posi��o e escala
            transform.localPosition = startPosition;
            ResetVisualElements();
        }

        private void ResetVisualElements()
        {
            if (visualElements != null && originalScales != null)
            {
                for (int i = 0; i < visualElements.Length && i < originalScales.Length; i++)
                {
                    if (visualElements[i] != null)
                    {
                        visualElements[i].transform.localScale = originalScales[i];
                    }
                }
            }
        }

        private IEnumerator BobbingAnimation()
        {
            while (isActive)
            {
                float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);

                if (rotateIndicator)
                {
                    transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                }

                yield return null;
            }
        }

        private IEnumerator ScalingAnimation()
        {
            while (isActive)
            {
                float time = 0f;

                while (time < animationDuration)
                {
                    float progress = time / animationDuration;
                    float scaleMultiplier = scaleAnimation.Evaluate(progress);

                    if (visualElements != null)
                    {
                        for (int i = 0; i < visualElements.Length; i++)
                        {
                            if (visualElements[i] != null && i < originalScales.Length)
                            {
                                visualElements[i].transform.localScale = originalScales[i] * scaleMultiplier;
                            }
                        }
                    }

                    time += Time.deltaTime;
                    yield return null;
                }

                if (!loopAnimation)
                    break;
            }
        }

        private IEnumerator RotatingAnimation()
        {
            while (isActive)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                yield return null;
            }
        }

        private IEnumerator PulsingAnimation()
        {
            while (isActive)
            {
                // Combina bobbing com scaling
                float time = Time.time;
                float bobOffset = Mathf.Sin(time * bobSpeed) * bobHeight;
                float scaleMultiplier = 1f + Mathf.Sin(time * bobSpeed * 0.5f) * 0.2f;

                // Aplica bobbing
                transform.localPosition = startPosition + Vector3.up * bobOffset;

                // Aplica scaling
                if (visualElements != null)
                {
                    for (int i = 0; i < visualElements.Length; i++)
                    {
                        if (visualElements[i] != null && i < originalScales.Length)
                        {
                            visualElements[i].transform.localScale = originalScales[i] * scaleMultiplier;
                        }
                    }
                }

                // Rota��o opcional
                if (rotateIndicator)
                {
                    transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                }

                yield return null;
            }
        }

        /// <summary>
        /// Muda o tipo de indicador em runtime
        /// </summary>
        public void SetIndicatorType(IndicatorType newType)
        {
            bool wasActive = isActive;

            if (isActive)
                SetActive(false);

            indicatorType = newType;

            if (wasActive)
                SetActive(true);
        }

        /// <summary>
        /// Pisca o indicador uma vez (�til para chamar aten��o)
        /// </summary>
        public void Blink(float duration = 0.5f)
        {
            if (!isActive)
                StartCoroutine(BlinkOnce(duration));
        }

        private IEnumerator BlinkOnce(float duration)
        {
            SetActive(true);
            yield return new WaitForSeconds(duration);
            SetActive(false);
        }

        #region Editor Helpers
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ajuda no editor a visualizar mudan�as
            if (Application.isPlaying && isActive)
            {
                SetActive(false);
                SetActive(true);
            }
        }

        [ContextMenu("Test Activate")]
        private void TestActivate()
        {
            SetActive(true);
        }

        [ContextMenu("Test Deactivate")]
        private void TestDeactivate()
        {
            SetActive(false);
        }

        [ContextMenu("Test Blink")]
        private void TestBlink()
        {
            Blink(1f);
        }
#endif
        #endregion
    }
}