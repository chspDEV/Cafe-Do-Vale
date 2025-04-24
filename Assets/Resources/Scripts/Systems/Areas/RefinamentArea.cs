using System.Collections;
using Tcp4.Assets.Resources.Scripts.UI;
using UnityEngine;

namespace Tcp4.Assets.Resources.Scripts.Systems.Areas
{
    public class RefinamentArea : MonoBehaviour
    {
        [SerializeField] private RawProduct expectedProduct;
        [SerializeField] private float refinementTime = 5f;
        [SerializeField] private Transform pointToSpawn;
        [SerializeField] private AnimationExecute anim;

        private bool isPlayerInArea = false;
        private bool isRefining = false;
        private bool isReady = false;
        private Inventory playerInventory;

        ImageToFill timeImage;

        public void DecreaseRefinementTime(float value)
        {
            refinementTime -= value;
            refinementTime = Mathf.Clamp(refinementTime, 0.5f, 5f);
            timeImage.SetupMaxTime(refinementTime);
        }

        private void Start()
        {
            var ui = UIManager.Instance;
            var obj = Instantiate(ui.pfImageToFill, ui.worldCanvas.gameObject.transform);
            timeImage = obj.GetComponent<ImageToFill>();
            ui.PlaceInWorld(pointToSpawn, timeImage.GetRectTransform());
            timeImage.SetupMaxTime(refinementTime);
        }

        private void Update()
        {
            UpdateTimeImage();
        }

        private void UpdateTimeImage()
        {
            if (isRefining) { timeImage.ChangeSprite(UIManager.Instance.sprRefinamentWait); }
            else if (!isRefining && isReady) { timeImage.ChangeSprite(UIManager.Instance.ready); }
            else { timeImage.ChangeSprite(UIManager.Instance.transparent); }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInArea = true;
                playerInventory = other.GetComponent<Inventory>();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (isPlayerInArea && !isRefining && !isReady && playerInventory != null && playerInventory.GetInventory().Contains(expectedProduct) )
            {
                StartCoroutine(RefineProduct());

                if(anim != null)
                    anim.ExecuteAnimation("Refinar");
            }
            else if (isPlayerInArea && playerInventory != null && isReady && !isRefining)
            {
                Collect();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInArea = false;
                isRefining = false;
                isReady = false;
                playerInventory = null;
                StopAllCoroutines();
            }
        }

        private IEnumerator RefineProduct()
        {
            SoundManager.PlaySound(SoundType.moendo, 0.08f);

            isRefining = true;
            float elapsedTime = 0f;

            while (elapsedTime < refinementTime)
            {
                elapsedTime += Time.deltaTime;
                timeImage.UpdateFill(elapsedTime);
                yield return null;
            }

            //yield return new WaitForSeconds(refinementTime);

            isRefining = false;
            isReady = true;
  
        }

        private void Collect()
        {
            if (playerInventory != null && playerInventory.GetInventory().Contains(expectedProduct))
            {
                playerInventory.RefineProduct(expectedProduct);
                isReady = false;
                isRefining = false;
            }
            else
            {
                Debug.Log("Algo deu errado");
            }

        }
    }
}
