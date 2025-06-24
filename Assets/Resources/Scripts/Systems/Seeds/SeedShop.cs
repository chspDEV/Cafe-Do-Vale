using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Tcp4;
using Tcp4.Assets.Resources.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeedShop : BaseInteractable
{
    [SerializeField] private Transform seedContainer;
    [SerializeField] private SeedShopItem seedShopItemPrefab;

    private List<GameObject> instances = new();

    public override void Start()
    {
        base.Start();
        PopulateShop();
    }

    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.H) && GameAssets.Instance.isDebugMode)
        {
            foreach (var item in instances)
            {
                var debug = item.GetComponent<SeedShopItem>();
                debug.OnBuyClicked();
            }

            PopulateShop();
        }
        

    }

    public override void OnInteract()
    {
        base.OnInteract();

        UIManager.Instance.ControlSeedShop(true);
        //DisableInteraction();

    }

    public override void OnLostFocus()
    {
        base.OnLostFocus();

        UIManager.Instance.ControlSeedShop(false);
        //EnableInteraction();

    }



    public void PopulateShop()
    {
        if (instances != null)
        {
            foreach (GameObject instanceGO in instances)
            {
                if (instanceGO != null) 
                {
                    Destroy(instanceGO);
                }
            }
            instances.Clear(); 
        }
        else
        {
            instances = new List<GameObject>(); 
        }

        List<Seed> unlockedSeeds = new();
        if (SeedManager.Instance != null && UnlockManager.Instance != null)
        {
            foreach (Seed seed in SeedManager.Instance.GetAllSeeds)
            {
                if (UnlockManager.Instance.IsProductionUnlocked(seed.targetProduction))
                {
                    unlockedSeeds.Add(seed);
                }
            }
        }
        else
        {
            Debug.LogError("seedmanager ou unlockmanager nao foi encontrado."); 
            return; 
        }

        List<Seed> seedsToShowInShop = new();
        int maxItemsInShop = 4;

        if (unlockedSeeds.Count > 0)
        {
            System.Random rng = new();
            List<Seed> shuffledUnlockedSeeds = unlockedSeeds.OrderBy(s => rng.Next()).ToList();

            for (int i = 0; i < maxItemsInShop; i++)
            {
                seedsToShowInShop.Add(shuffledUnlockedSeeds[Random.Range(0,shuffledUnlockedSeeds.Count)]);
            }
        }

        if (seedShopItemPrefab == null)
        {
            Debug.LogError("prefab do item da loja de sementes (seedshopitemprefab) nao esta atribuido."); 
            return;
        }
        if (seedContainer == null)
        {
            Debug.LogError("container das sementes (seedcontainer) nao esta atribuido."); 
            return;
        }

        foreach (Seed seedToDisplay in seedsToShowInShop)
        {
            SeedShopItem shopItemInstance = Instantiate(seedShopItemPrefab, seedContainer);
            shopItemInstance.Configure(seedToDisplay);
            instances.Add(shopItemInstance.gameObject); 
        }
    }
}