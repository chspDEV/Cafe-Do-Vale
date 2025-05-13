using Tcp4.Assets.Resources.Scripts.Managers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Tcp4.Assets.Resources.Scripts.Systems.Areas
{
    public class BuyCollectArea: MonoBehaviour, IUpgradable
    {
        [SerializeField] private ImageToFill priceImage;
        [SerializeField] float stackedMoney = 0f;
        [SerializeField] float price = 50f;

        [SerializeField] int decreaseValue = 1;
        [SerializeField] bool canUpgrade = true;
        [SerializeField] GameObject pfCollectArea;
        [SerializeField] Transform pointToImage;

        void Start()
        {
            
            priceImage = UIManager.Instance.PlaceFillImage(pointToImage);
            priceImage.ChangeSprite(GameAssets.Instance.Money);
            priceImage.SetupMaxTime(price);
            priceImage.UpdateFill(stackedMoney);
        }

        public void IncreasePrice()
        {
            //Nesse Caso aqui nao precisa de upgrade
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
            ShopManager.Instance.TrySpendMoney(decreaseValue);

            OnChangePrice();

            if(stackedMoney >= price)
            {
                OnUpgrade();
            }
        }

        public void OnUpgrade()
        {
            canUpgrade = false;
            Instantiate(pfCollectArea, transform.position, quaternion.identity);
            Destroy(this.gameObject);
            Destroy(priceImage.gameObject);
        }
    }

    
}
