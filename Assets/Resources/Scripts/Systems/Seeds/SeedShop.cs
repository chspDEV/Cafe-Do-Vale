using Sirenix.OdinInspector;
using Tcp4;
using Tcp4.Assets.Resources.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeedShop : BaseInteractable
{
    [SerializeField] private Transform seedContainer;
    [SerializeField] private SeedShopItem seedShopItemPrefab;

    public override void Start()
    {
        base.Start();
        PopulateShop();
    }

    public override void OnInteract()
    {
        base.OnInteract();
        UIManager.Instance.ControlSeedShop(true);
    }

    public override void OnLostFocus()
    {
        base.OnLostFocus();
        UIManager.Instance.ControlSeedShop(false);
    }

    [Button]
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