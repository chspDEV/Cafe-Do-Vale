using System.Collections;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Tcp4
{
    public class UpdateImageByInput : MonoBehaviour
    {
        public Image image;

        public Sprite xboxTargetSprite;
        public Sprite playstationTargetSprite;
        public Sprite pcTargetSprite;

        private GameAssets reference;

        public void Start()
        {
            StartCoroutine(InitializeSettings(0.25f));
        }

        private IEnumerator InitializeSettings(float delay)
        {
            yield return new WaitForSeconds(delay);
            reference = GameAssets.Instance;

            if (reference != null)
            {
                reference.OnChangeInteractionSprite += UpdateSpriteInteraction;
                UpdateSpriteInteraction();
                //Debug.Log("Encontrei o gameAssets!");
            }
            else
            {
                Debug.LogError("Não encontrei o gameAssets!");
            }

            yield return null;

        }


        void UpdateSpriteInteraction()
        {
            switch (reference.currentInputType)
            {
                case CurrentInputType.PC:
                    image.sprite = pcTargetSprite;
                    break;
                case CurrentInputType.XBOX:
                    image.sprite = xboxTargetSprite;
                    break;
                case CurrentInputType.PLAYSTATION:
                    image.sprite = playstationTargetSprite;
                    break;
                case CurrentInputType.NONE:
                    StartCoroutine(InitializeSettings(0.33f));
                    break;
                default:
                    StartCoroutine(InitializeSettings(0.33f));
                    break;
            }

        }
    }
}
