using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tcp4.Assets.Resources.Scripts.Managers;
using GameResources.Project.Scripts.Utilities.Audio;

public class SeedShopItem : MonoBehaviour
{
    [SerializeField] private Image seedIcon;
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private Button buyButton;

    private Seed seed;
    private string item_id;

    public void Configure(Seed seedData)
    {
        seed = seedData;
        seedIcon.sprite = seed.seedIcon;
        displayText.text = $"{seed.seedName}\n\n" +"R$ " + seed.purchaseCost.ToString();
        buyButton.onClick.AddListener(OnBuyClicked);
        item_id = seed.seedName;
    }

    public void OnBuyClicked()
    {
        if (ShopManager.Instance.TrySpendMoney(seed.purchaseCost))
        {
            SeedManager.Instance.AddSeed(seed.targetProduction);
            InteractionManager.Instance.UpdateLastId(item_id);

            //Fazendo o request de sfx
            SoundEventArgs sfxArgs = new()
            {
                Category = SoundEventArgs.SoundCategory.SFX,
                AudioID = "comprar_01", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                VolumeScale = .5f, // Escala de volume (opcional, padrão é 1f)
                Pitch = 1

            };
            SoundEvent.RequestSound(sfxArgs);

            Destroy(this.gameObject);
        }
        else { Debug.LogError("Nao foi possivel comprar a semente!"); }
    }
}