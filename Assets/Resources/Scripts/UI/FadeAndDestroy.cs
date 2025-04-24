using UnityEngine;

namespace Tcp4
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeAndDestroy : MonoBehaviour
    {
        [Tooltip("Tempo em segundos para o fade completo")]
        public float fadeDuration = 1f;

        private CanvasGroup canvasGroup;
        private float timer = 0f;
        private float startAlpha;

        void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            startAlpha = canvasGroup.alpha;

            if (fadeDuration <= 0f)
            {
                Debug.LogError("Fade duration deve ser maior que zero!", this);
                enabled = false;
            }
        }

        void Update()
        {
            timer += Time.deltaTime;

            if (timer >= fadeDuration)
            {
                // Garante o alpha zero e destrói
                canvasGroup.alpha = 0f;
                Destroy(gameObject);
                return;
            }

            // Calcula o novo alpha usando interpolação linear
            float progress = timer / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
        }
    }
}