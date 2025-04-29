using System.Collections;
using Tcp4;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;
public class BuyUpgradeCups : MonoBehaviour, IUpgradable
{
    [SerializeField] private ImageToFill priceImage;
        [SerializeField] float stackedMoney = 0f;
        [SerializeField] float price = 50f;
        [SerializeField] float priceMultiplier = 1.5f;
        [SerializeField] int decreaseValue = 1;
        [SerializeField] bool canUpgrade = true;
        [SerializeField] Transform pointToImage;
        [SerializeField] int upgrades = 0;
        [SerializeField] int maxUpgrades = 2;

        private float cooldownToBuyAgain = 5f;


        void Start()
        {
            priceImage = UIManager.Instance.PlaceFillImage(pointToImage);
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
            
            ShopManager.Instance.IncreaseCupLevel();

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