using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tcp4.Assets.Resources.Scripts.Managers;
using GameResources.Project.Scripts.Utilities.Audio;
using System.Collections;

public class SeedShopItem : MonoBehaviour
{
    [SerializeField] private Image seedIcon;
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private Button buyButton;

    private Seed seed;
    private string item_id;

    // CORREÇÃO 1: Aumenta o tempo de proteção
    private static float timeSinceShopOpened;
    private const float PROTECTION_TIME = 0.8f; // Tempo em segundos para proteger contra cliques acidentais

    public void Configure(Seed seedData)
    {
        seed = seedData;
        seedIcon.sprite = seed.seedIcon;
        displayText.text = $"{seed.seedName}\n\n" + "R$ " + seed.purchaseCost.ToString();
        buyButton.onClick.AddListener(OnBuyClicked);
        item_id = seed.seedName;

        // CORREÇÃO 2: Garante que o botão comece desabilitado
        buyButton.interactable = false;
    }

    public static void NotifyShopOpened()
    {
        timeSinceShopOpened = Time.unscaledTime;
    }

    public void OnBuyClicked()
    {
        // CORREÇÃO 3: Aumenta a proteção contra clique acidental instantâneo
        if (Time.unscaledTime - timeSinceShopOpened < PROTECTION_TIME)
        {
            Debug.Log($"Clique bloqueado! Tempo desde abertura: {Time.unscaledTime - timeSinceShopOpened:F2}s");
            return;
        }

        // CORREÇÃO 4: Verifica se o botão está realmente interativo
        if (!buyButton.interactable)
        {
            Debug.Log("Botão não está interativo!");
            return;
        }

        if (ShopManager.Instance.TrySpendMoney(seed.purchaseCost))
        {
            // CORREÇÃO 5: Desabilita o botão imediatamente após a compra
            buyButton.interactable = false;

            SeedManager.Instance.AddSeed(seed.targetProduction);
            InteractionManager.Instance.UpdateLastId(item_id);

            // SFX
            SoundEventArgs sfxArgs = new()
            {
                Category = SoundEventArgs.SoundCategory.SFX,
                AudioID = "comprar_01",
                VolumeScale = .5f,
                Pitch = 1
            };
            SoundEvent.RequestSound(sfxArgs);

            SeedShop.TriggerOnBuyed();
            Destroy(this.gameObject);
        }
        else
        {
            Debug.LogError("Nao foi possivel comprar a semente!");
        }
    }
}