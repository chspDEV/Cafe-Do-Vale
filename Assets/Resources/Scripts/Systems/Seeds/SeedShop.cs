using Sirenix.OdinInspector;
using System;
using System.Collections;
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

    static event Action OnBuyed;
    public static void TriggerOnBuyed() => OnBuyed?.Invoke();

    public override void Start()
    {
        base.Start();
        interactable_id = "seedShop";
        PopulateShop();

        OnBuyed += SelectFistButton;
    }

    private void OnDisable()
    {
        OnBuyed -= SelectFistButton;
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
        if (UIManager.Instance.HasMenuOpen()) return;

        base.OnInteract();

        // CORREÇÃO 1: Notifica que a loja foi aberta (para proteção de timing)
        SeedShopItem.NotifyShopOpened();

        // CORREÇÃO 2: Aumenta o tempo de bloqueio para evitar cliques acidentais
        EventSystemBlocker.BlockForSeconds(0.8f);

        UIManager.Instance.ControlSeedShop(true);
    }

    public override void OnLostFocus()
    {
        base.OnLostFocus();

        UIManager.Instance.ControlSeedShop(false);
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
                seedsToShowInShop.Add(shuffledUnlockedSeeds[UnityEngine.Random.Range(0, shuffledUnlockedSeeds.Count)]);
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

        // CORREÇÃO 3: Aumenta o delay para garantir que tudo esteja inicializado
        StartCoroutine(EnableButtonsWithDelay(0.5f));

        // CORREÇÃO 4: Remove chamadas duplicadas de SelectFistButton
        // SelectFistButton(); - REMOVIDO
        // SelectFistButton(); - REMOVIDO
    }

    private IEnumerator EnableButtonsWithDelay(float delay)
    {
        foreach (var go in instances)
        {
            if (go != null)
            {
                var button = go.GetComponentInChildren<Button>();
                if (button != null)
                {
                    button.interactable = false;
                }
            }
        }

        yield return new WaitForSecondsRealtime(delay);

        foreach (var go in instances)
        {
            if (go != null)
            {
                var button = go.GetComponentInChildren<Button>();
                if (button != null)
                {
                    button.interactable = true;
                }
            }
        }

        // CORREÇÃO 5: Só seleciona o botão após habilitar os botões
        SelectFistButton();
    }

    public void SelectFistButton()
    {
        StartCoroutine(SelectButtonNextFrame());
    }

    private IEnumerator SelectButtonNextFrame()
    {
        // CORREÇÃO 6: Aumenta o tempo de espera
        yield return new WaitForSecondsRealtime(0.1f);

        if (instances.Count > 0)
        {
            // Remove objetos destruídos da lista
            instances.RemoveAll(go => go == null);

            if (instances.Count == 0)
                yield break;

            var seedShopInteraction = UIManager.Instance.seedShopInteraction;
            if (seedShopInteraction != null && instances[0] != null)
            {
                seedShopInteraction.SetSelectable(instances[0]);
            }
        }
    }
}