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
        private CollectArea reference;

        public void SetColletArea(CollectArea collectarea) => reference = collectarea;

        public void ConfigureVisuals()
        {
            cardImage.sprite = myProduction.product.productImage;
            cardName.text = myProduction.product.productName;
        }

        public void Setup()
        {
            ProductionManager.Instance.SetupNewProduction(myProduction);
            ProductionManager.Instance.InvokeChooseProduction();
        }


    }
}