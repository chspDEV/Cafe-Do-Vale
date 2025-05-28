// SeedManager.cs
using System.Collections.Generic;
using Tcp4;
using UnityEngine;

public class SeedManager : Singleton<SeedManager>
{
    [SerializeField] private List<Seed> allSeeds;
    private Dictionary<Production, int> seedInventory = new Dictionary<Production, int>();

    public SeedShop seedShop;

    public void AddSeed(Production production, int amount = 1)
    {
        if (seedInventory.ContainsKey(production))
        {
            seedInventory[production] += amount;
        }
        else
        {
            seedInventory.Add(production, amount);
        }
        UpdateUI();
    }

    public Dictionary<Production, int> GetInventory => seedInventory;
    public List<Seed> GetAllSeeds => allSeeds;

    public bool HasSeed(Production production)
    {
        return seedInventory.ContainsKey(production) && seedInventory[production] > 0;
    }

    public void ConsumeSeed(Production production)
    {
        if (HasSeed(production))
        {
            seedInventory[production]--;
            UpdateUI();
        }
    }

    public Seed GetSeedForProduction(Production production)
    {
        return allSeeds.Find(seed => seed.targetProduction == production);
    }

    private void UpdateUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateSeedInventoryView();
        }
    }
}