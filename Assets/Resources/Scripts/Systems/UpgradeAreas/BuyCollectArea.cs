using TMPro;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

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
                priceImage.ChangeBillboard(Vector3.zero, new Vector3(180f, 0f, 0f));
                priceImage.SetupMaxTime(price);

                //priceText = UIManager.Instance.PlaceTextProgress(pointToImage, price);

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
                UpdatePriceDisplay();
                return;
            }

            // Pega o menor valor entre: o que falta e o que o jogador tem
            float amountToSpend = Mathf.Min(remaining, playerMoney);

            CompletePurchase(Mathf.CeilToInt(amountToSpend));

            // Feedback visual quando não completar
            if (amountToSpend < remaining)
            {
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
            Destroy(gameObject);
        }

   
    }
}