using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameResources.Resources.Scripts.Systems.Interaction
{
    public class InteractionPopUp : MonoBehaviour
    {
        [SerializeField] private float showDuration = 0.3f;
        [SerializeField] private float hideDuration = 0.2f;
        [SerializeField] private float popUpHeight = 1f;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI popUpText;
        
        private Vector3 targetPosition;
        private Vector3 originalScale;
        private bool isVisible;
        private Sequence currentSequence;
        private bool isAnimating;
        private Camera mainCamera;
        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            AlignToCamera();
        }
        private void Start()
        {
            targetPosition = transform.position + Vector3.up * popUpHeight;
            originalScale = rectTransform.localScale;
            SetAlpha(0f);
            rectTransform.position = transform.position;
            isVisible = false;
        }

        private void AlignToCamera()
        {
            if (mainCamera != null)
            {
                Vector3 lookDirection = mainCamera.transform.forward;
                lookDirection.y = 0;
                lookDirection.Normalize();
                transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            }
        }
        public void Show()
        {
            if (isVisible || isAnimating) return;

            CleanupTween();
            isAnimating = true;
            
            rectTransform.position = transform.position;
            rectTransform.localScale = originalScale * 0.5f;
            SetAlpha(0f);
            rectTransform.DOKill();
            currentSequence = DOTween.Sequence();
            currentSequence
                .SetEase(Ease.OutBack)
                .Join(rectTransform.DOMove(targetPosition, showDuration))
                .Join(rectTransform.DOScale(originalScale, showDuration))
                .OnUpdate(() => 
                {
                    if (currentSequence != null)
                    {
                        float progress = currentSequence.ElapsedPercentage();
                        SetAlpha(progress);
                    }
                })
                .OnComplete(() => 
                {
                    isVisible = true;
                    isAnimating = false;
                });
        }

        public void Hide()
        {
            if (!isVisible && !isAnimating) return;

            CleanupTween();
            isAnimating = true;

            currentSequence = DOTween.Sequence();
            currentSequence
                .SetEase(Ease.InBack)
                .Join(rectTransform.DOMove(transform.position, hideDuration))
                .Join(rectTransform.DOScale(originalScale * 0.5f, hideDuration))
                .OnUpdate(() => 
                {
                    if (currentSequence != null)
                    {
                        float progress = 1 - currentSequence.ElapsedPercentage();
                        SetAlpha(progress);
                    }
                })
                .OnComplete(() => 
                {
                    isVisible = false;
                    isAnimating = false;
                    rectTransform.position = transform.position;
                });
        }

        private void SetAlpha(float alpha)
        {
            if (backgroundImage != null)
            {
                Color bgColor = backgroundImage.color;
                bgColor.a = alpha;
                backgroundImage.color = bgColor;
            }

            if (popUpText != null)
            {
                Color textColor = popUpText.color;
                textColor.a = alpha;
                popUpText.color = textColor;
            }
        }

        private void CleanupTween()
        {
            if (currentSequence != null)
            {
                if (currentSequence.IsActive())
                {
                    currentSequence.Kill();
                }
                currentSequence = null;
            }
        }

        private void OnDestroy()
        {
            CleanupTween();
        }
    }
}