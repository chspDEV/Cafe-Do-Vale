using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tcp4.Assets.Resources.Scripts.Managers;

namespace Tcp4
{
    public class SeedInventoryCard : MonoBehaviour
    {
        public Production myProduction;

        public Image seedImage;
        public TextMeshProUGUI seedNameText;
        [SerializeField] private TextMeshProUGUI seedCountText;

        public void Configure(Production production, int count)
        {
            myProduction = production;
            seedImage.sprite = production.outputProduct.productImage;
            seedNameText.text = production.outputProduct.productName;
            seedCountText.text = count + "x";
        }
    }
}

