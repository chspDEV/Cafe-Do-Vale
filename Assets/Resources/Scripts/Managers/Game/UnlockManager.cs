using System;
using System.Collections.Generic;
using System.Linq;
using Tcp4.Assets.Resources.Scripts.Managers;
using Tcp4;
using UnityEngine;

[System.Serializable]
public class UnlockableItem<T>
{
    public T item;
    public int requiredReputationLevel;
    public bool isUnlocked = false;

    [Header("Notification Settings")]
    public string notificationTitle;
    public string notificationMessage;
    public Sprite notificationIcon;

    void OnValidate()
    {
        isUnlocked = false; // Reset unlocked state on validation
    }
}

public class UnlockManager : Singleton<UnlockManager>
{
    [SerializeField] public UnlockConfig config;
    [SerializeField] private int currentReputationLevel;
    public int CurrentReputationLevel => currentReputationLevel;

    // EVENTO PÚBLICO PARA NOTIFICAR SOBRE DESBLOQUEIOS
    public event Action<string, string, Sprite> OnItemUnlocked;

    public event Action OnReputationChanged;
    public event Action OnProductionsUpdated;
    public event Action OnDrinksUpdated;
    public event Action OnCupUpgraded;

    private ShopManager shopManager;
    private UIManager uiManager;
    public List<Drink> CurrentMenu { get; private set; } = new();

    private void Start()
    {
        uiManager = UIManager.Instance;
        shopManager = ShopManager.Instance;
        shopManager.OnChangeStar += HandleReputationUpdate;

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

                string title = string.IsNullOrEmpty(item.notificationTitle)
                    ? $"{item.item.ToString()} desbloqueado!"
                    : item.notificationTitle;

                OnItemUnlocked?.Invoke(title, item.notificationMessage, item.notificationIcon);
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

    /// <summary>
    /// Usado pelo SaveManager para restaurar reputação do save.
    /// </summary>
    public void SetReputation(int level)
    {
        currentReputationLevel = level;
    }
}
