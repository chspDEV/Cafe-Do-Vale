using System;
using System.Collections.Generic;
using Tcp4.Assets.Resources.Scripts.Managers;
using Tcp4;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class UnlockableItem<T>
{
    public T item;
    public int requiredReputationLevel;
    public bool isUnlocked;
}

[CreateAssetMenu(menuName = "Unlock System/Unlock Config")]
public class UnlockConfig : ScriptableObject
{
    public List<UnlockableItem<Production>> unlockableProductions = new();
    public List<UnlockableItem<Drink>> unlockableDrinks = new();
    public List<UnlockableItem<GameObject>> unlockableCups = new();
}

public class UnlockManager : Singleton<UnlockManager>
{
    [SerializeField] private UnlockConfig config;
    [SerializeField] private int currentReputationLevel;

    public event Action OnReputationChanged;
    public event Action OnProductionsUpdated;
    public event Action OnDrinksUpdated;
    public event Action OnCupUpgraded;

    private ShopManager shopManager;
    private UIManager uiManager;
    public List<Drink> CurrentMenu { get; private set; } = new();

    

    protected override void Awake()
    {
        base.Awake();
        shopManager = ShopManager.Instance;
        shopManager.OnChangeStar += HandleReputationUpdate;
    }

    private void Start()
    {
        uiManager = UIManager.Instance;
        InitializeUnlockables();
    }

    private void InitializeUnlockables()
    {
        UpdateUnlockStates();
        UpdateAvailableContent();
    }

    private void HandleReputationUpdate()
    {
        int newLevel = CalculateReputationLevel();
        if (newLevel != currentReputationLevel)
        {
            currentReputationLevel = newLevel;
            UpdateUnlockStates();
            UpdateAvailableContent();
            OnReputationChanged?.Invoke();
        }
    }

    private int CalculateReputationLevel()
    {
        float reputationProgress = Mathf.Clamp01(shopManager.GetStars() / shopManager.GetMaxStars());
        return Mathf.FloorToInt(reputationProgress * 5f); // 0-5 níveis
    }

    private void UpdateUnlockStates()
    {
        CheckAndUnlock(config.unlockableProductions);
        CheckAndUnlock(config.unlockableDrinks);
        CheckAndUnlock(config.unlockableCups);
    }

    private void CheckAndUnlock<T>(List<UnlockableItem<T>> items)
    {
        foreach (var item in items)
        {
            if (!item.isUnlocked && currentReputationLevel >= item.requiredReputationLevel)
            {
                item.isUnlocked = true;
            }
        }
    }

    private void UpdateAvailableContent()
    {
        UpdateDrinkMenu();
        UpdateProductions();
        UpdateCupVisual();
    }

    private void UpdateDrinkMenu()
    {
        CurrentMenu.Clear();

        foreach (var drink in config.unlockableDrinks)
        {
            if (drink.isUnlocked) CurrentMenu.Add(drink.item);
        }

        OnDrinksUpdated?.Invoke();
    }

    public List<Drink> GetCurrentMenu() => new List<Drink>(CurrentMenu);


    private void UpdateProductions()
    {
        List<Production> unlockedProductions = new();
        foreach (var production in config.unlockableProductions)
        {
            if (production.isUnlocked) unlockedProductions.Add(production.item);
        }

        uiManager.ClearProductionCards();
        foreach (Production p in unlockedProductions)
        {
            uiManager.CreateNewProductionCard(p);
        }
        OnProductionsUpdated?.Invoke();
    }

    private void UpdateCupVisual()
    {
        int highestCupLevel = 0;
        foreach (var cup in config.unlockableCups)
        {
            if (cup.isUnlocked && cup.requiredReputationLevel > highestCupLevel)
            {
                highestCupLevel = cup.requiredReputationLevel;
            }
        }
        shopManager.cupLevel = highestCupLevel;
        OnCupUpgraded?.Invoke();
    }

    public bool IsProductionUnlocked(Production production)
    {
        foreach (var item in config.unlockableProductions)
        {
            if (item.item == production) return item.isUnlocked;
        }
        return false;
    }

    public bool IsDrinkUnlocked(Drink drink)
    {
        foreach (var item in config.unlockableDrinks)
        {
            if (item.item == drink) return item.isUnlocked;
        }
        return false;
    }

    public void UnlockNextCupLevel()
    {
        foreach (var cup in config.unlockableCups)
        {
            if (!cup.isUnlocked && currentReputationLevel >= cup.requiredReputationLevel)
            {
                cup.isUnlocked = true;
                UpdateCupVisual();
                return;
            }
        }
    }

    public int GetCurrentReputationLevel() => currentReputationLevel;
}