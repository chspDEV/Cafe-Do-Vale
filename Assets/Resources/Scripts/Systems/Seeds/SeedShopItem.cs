using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tcp4.Assets.Resources.Scripts.Managers;

public class SeedShopItem : MonoBehaviour
{
    [SerializeField] private Image seedIcon;
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private Button buyButton;

    private Seed seed;

    public void Configure(Seed seedData)
    {
        seed = seedData;
        seedIcon.sprite = seed.seedIcon;
        displayText.text = $"{seed.seedName}\n\n" +"R$ " + seed.purchaseCost.ToString();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    private void OnBuyClicked()
    {
        if (ShopManager.Instance.TrySpendMoney(seed.purchaseCost))
        {
            SeedManager.Instance.AddSeed(seed.targetProduction);
            Destroy(this.gameObject);
        }
    }
}