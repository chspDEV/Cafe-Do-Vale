using Tcp4.Assets.Resources.Scripts.Managers;
using Tcp4.Assets.Resources.Scripts.UI;
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
            ShopManager.Instance.DecreaseMoney(decreaseValue);

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
