using UnityEngine;

namespace Tcp4
{
    public class UI_Fade : MonoBehaviour
    {
        public CanvasGroup canvasGroup;
        public float fadeIncrease = 1f;
        private bool isFadingIn = false;

        void OnDisable()
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 0;
        }

        void OnEnable()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                isFadingIn = true;
            }
        }

        void Update()
        {
            if (isFadingIn && canvasGroup != null)
            {
                canvasGroup.alpha += fadeIncrease * Time.deltaTime;
                if (canvasGroup.alpha >= 1f)
                {
                    canvasGroup.alpha = 1f;
                    isFadingIn = false;
                }
            }
        }
    }
}
