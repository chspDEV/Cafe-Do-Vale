using TMPro;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;
using GameResources.Project.Scripts.Utilities.Audio;

namespace Tcp4.Assets.Resources.Scripts.Systems.Areas
{
    public class BuyCollectArea : BaseInteractable
    {
        [Header("UI References")]
        [SerializeField] private ImageToFill priceImage;
        [SerializeField] private TextToProgress priceText;
        

        [Header("Configurações")]
        [SerializeField] private int stackedMoney = 0;
        [SerializeField] private int price = 50;
        [SerializeField] private bool canUpgrade = true;

        [Header("Prefabs")]
        [SerializeField] private GameObject pfCollectArea;
        [SerializeField] private Transform pointToImage;

        public override void Start()
        {
            base.Start();
            InitializeUI();
            UpdatePriceDisplay();
        }

        private void InitializeUI()
        {
            if (UIManager.Instance != null)
            {
                priceImage = UIManager.Instance.PlaceFillImage(pointToImage);
                priceImage.ChangeSprite(GameAssets.Instance.Money);
                priceImage.ChangeFillStart(0);
                priceImage.ChangeSize(new Vector3(0.5f, 1f, 1f));
                priceImage.SetupMaxTime(price);

                priceText = UIManager.Instance.PlaceTextProgress(pointToImage, price);
                priceText.ChangeBillboard(new Vector3(0f, -0.7f, 0f), new Vector3(35f, 180f, 0f));
            }
        }

        public override void OnInteract()
        {
            base.OnInteract();
            TryUpgrade();
        }

        private void TryUpgrade()
        {
            if (!canUpgrade) return;

            float remaining = price - stackedMoney;
            float playerMoney = ShopManager.Instance.GetMoney();

            if (playerMoney <= 0)
            {
                Debug.Log("Sem dinheiro suficiente!");

                SoundEventArgs sfxArgs = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "erro", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                    VolumeScale = .8f, // Escala de volume (opcional, padrão é 1f)

                };
                SoundEvent.RequestSound(sfxArgs);

                UpdatePriceDisplay();
                return;
            }
            float amountToSpend = Mathf.Min(remaining, playerMoney);

            CompletePurchase(Mathf.CeilToInt(amountToSpend));

            if (amountToSpend < remaining)
            {
                //BOTAR FEEDBACKS AQ
                Debug.Log($"Colocado {amountToSpend:C0} (Faltam {remaining - amountToSpend:C0})");
            }
        }

        private void CompletePurchase(int amount)
        {
            stackedMoney += amount;
            ShopManager.Instance.TrySpendMoney(amount);
            UpdatePriceDisplay();

            if (stackedMoney >= price)
            {
                OnUpgrade();
            }
        }

        private void UpdatePriceDisplay()
        {
            // Atualiza a imagem de preenchimento
            if (priceImage != null)
            {
                priceImage.UpdateFill(stackedMoney);
            }

            // Atualiza o texto com os valores
            if (priceText != null)
            {
                priceText.UpdateProgress(stackedMoney);
            }
        }

        public void OnUpgrade()
        {
            canUpgrade = false;
            Instantiate(pfCollectArea, transform.position, Quaternion.identity);
            CleanupBeforeDestroy();
        }

        private void CleanupBeforeDestroy()
        {
            if (priceImage != null)
            {
                Destroy(priceImage.gameObject);
            }

            if (priceText != null)
            {
                Destroy(priceText.gameObject);
            }

            Destroy(gameObject);
        }

   
    }
}