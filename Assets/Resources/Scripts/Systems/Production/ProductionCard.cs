using System.Collections;
using Tcp4.Assets.Resources.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tcp4
{
    public class ProductionCard : MonoBehaviour
    {
        public Production myProduction;

        public Image cardImage;
        public TextMeshProUGUI cardName;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private TextMeshProUGUI seedCountText;

        private CollectArea reference;

        public void SetColletArea(CollectArea collectarea) => reference = collectarea;

        public void ConfigureVisuals()
        {
            cardImage.sprite = myProduction.product.productImage;
            cardName.text = myProduction.product.productName;
        }

        private void UpdateUI()
        {
            bool hasSeed = SeedManager.Instance.HasSeed(myProduction);
            lockIcon.SetActive(!hasSeed);

            if (SeedManager.Instance.GetInventory.TryGetValue(myProduction, out int count))
            {
                seedCountText.text = count.ToString();
            }
        }

        public void Setup()
        {
            if (!SeedManager.Instance.HasSeed(myProduction))
            {
                Debug.Log("Você precisa de sementes para esta produção!");
                return;
            }

            SeedManager.Instance.ConsumeSeed(myProduction);
            ProductionManager.Instance.SetupNewProduction(myProduction);
            ProductionManager.Instance.InvokeChooseProduction();
            UpdateUI();
        }


    }
}