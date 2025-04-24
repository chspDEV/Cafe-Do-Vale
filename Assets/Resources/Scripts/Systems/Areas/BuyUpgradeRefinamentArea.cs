using System.Collections;
using System.Collections.Generic;
using Tcp4.Assets.Resources.Scripts.Managers;
using Tcp4.Assets.Resources.Scripts.UI;
using UnityEngine;

namespace Tcp4.Assets.Resources.Scripts.Systems.Areas
{
    public class BuyUpgradeRefinamentArea : MonoBehaviour, IUpgradable
    {
        [SerializeField] private ImageToFill priceImage;
        [SerializeField] float stackedMoney = 0f;
        [SerializeField] float price = 50f;
        [SerializeField] float priceMultiplier = 1.5f;

        [SerializeField] float decreaseValueToRefinament = 0.5f;
        [SerializeField] int decreaseValue = 1;
        [SerializeField] bool canUpgrade = true;
        [SerializeField] Transform pointToImage;
        [SerializeField] List<RefinamentArea> refinamentAreas;

        [SerializeField] int upgrades = 0;
        [SerializeField] int maxUpgrades = 5;

        private float cooldownToBuyAgain = 5f;


        void Start()
        {
            var ui = UIManager.Instance;
            var obj = Instantiate(ui.pfImageToFill, ui.worldCanvas.gameObject.transform);
            priceImage = obj.GetComponent<ImageToFill>();
            ui.PlaceInWorld(pointToImage, priceImage.GetRectTransform());
            priceImage.ChangeSprite(GameAssets.Instance.Money);
            priceImage.SetupMaxTime(price);
            priceImage.UpdateFill(stackedMoney);
        }

        public void IncreasePrice()
        {
            stackedMoney = 0f;
            price *= priceMultiplier;
            priceImage.SetupMaxTime(price);
        }

        public void OnChangePrice()
        {
            priceImage.UpdateFill(stackedMoney);
        }

        public void OnStackMoney()
        {
            if(!canUpgrade || ShopManager.Instance.GetMoney() < decreaseValue) 
            {
                Debug.Log($"Não é possivel acumular dinheiro em {name}!");
                return;
            }
            

            stackedMoney += decreaseValue;
            ShopManager.Instance.DecreaseMoney(decreaseValue);

            OnChangePrice();

            if(stackedMoney >= price)
            {
                OnUpgrade();
            }
        }

        public void OnUpgrade()
        {
            if (upgrades >= maxUpgrades)
                return;

            canUpgrade = false;
            upgrades++;

            foreach (RefinamentArea r in refinamentAreas)
            {
                r.DecreaseRefinementTime(decreaseValueToRefinament);
            }

            IncreasePrice();
            OnChangePrice();

            if (upgrades < maxUpgrades)
            {
                StartCoroutine(WaitCooldown());
            }
        }

        IEnumerator WaitCooldown()
        {
            yield return new WaitForSeconds(cooldownToBuyAgain);

            canUpgrade = true;
        }
    }
}
