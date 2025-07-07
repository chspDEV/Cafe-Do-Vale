using GameResources.Project.Scripts.Utilities.Audio;
using System.Collections;
using Tcp4.Assets.Resources.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tcp4
{
    public class ProductionCard : MonoBehaviour
    {
        public Production myProduction;

        public Image cardImage;
        public TextMeshProUGUI cardName;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private TextMeshProUGUI seedCountText;

        private ProductionArea reference;

        private string item_id;
        public void SetColletArea(ProductionArea collectarea) => reference = collectarea;

        public void ConfigureVisuals()
        {
            cardImage.sprite = myProduction.outputProduct.productImage;
            cardName.text = myProduction.outputProduct.productName;
            item_id = myProduction.outputProduct.productName;
        }

        private void OnEnable()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            bool hasSeed = SeedManager.Instance.HasSeed(myProduction);
            lockIcon.SetActive(!hasSeed);

            if (SeedManager.Instance.GetInventory.TryGetValue(myProduction, out int count))
            {
                seedCountText.text = count.ToString() + "x";
            }
            else
            {
                seedCountText.text = "0x";
            }
        }

        public void Setup()
        {
            if (!SeedManager.Instance.HasSeed(myProduction))
            {
                Debug.Log("Você precisa de sementes para esta produção!");
                SoundEventArgs sfxArgs = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "erro", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                    VolumeScale = .8f, // Escala de volume (opcional, padrão é 1f)

                };
                SoundEvent.RequestSound(sfxArgs);
                return;
            }

            InteractionManager.Instance.UpdateLastId(item_id);
            SeedManager.Instance.ConsumeSeed(myProduction);
            ProductionManager.Instance.SetupNewProduction(myProduction);
            ProductionManager.Instance.InvokeChooseProduction();
            UpdateUI();
        }


    }
}