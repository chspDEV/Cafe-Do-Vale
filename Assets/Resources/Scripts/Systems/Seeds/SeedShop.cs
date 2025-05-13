using Tcp4.Assets.Resources.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeedShop : MonoBehaviour
{
    [SerializeField] private Transform seedContainer;
    [SerializeField] private SeedShopItem seedShopItemPrefab;

    void Start()
    {
        PopulateShop();
    }

    private void PopulateShop()
    {
        foreach (Seed seed in SeedManager.Instance.GetAllSeeds)
        {
            if (!UnlockManager.Instance.IsProductionUnlocked(seed.targetProduction)) continue;

            var shopItem = Instantiate(seedShopItemPrefab, seedContainer);
            shopItem.Configure(seed);
        }
    }
}