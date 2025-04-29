using UnityEngine;
using UnityEngine.UI;


namespace Tcp4
{
    public class DebugInteractable : MonoBehaviour, IInteractable
    {
        private Image image;
        public Sprite interactionSprite;

        void Start()
        {
            image = UIManager.Instance.PlaceImage(gameObject.transform);
            image.sprite = interactionSprite;
            if (image != null)
            {
                image.enabled = false;
            }
        }


        public void OnFocus()
        {
            Debug.Log("Objeto em foco!");

            if (image != null)
            {
                image.enabled = true;
            }
        }

        public void OnLostFocus()
        {
            Debug.Log("Objeto fora de foco!");

            if (image != null)
            {
                image.enabled = false;
            }
        }

        public void OnInteract()
        {
            Debug.Log($"Interagi com {gameObject.name}!");
        }
    }
}
