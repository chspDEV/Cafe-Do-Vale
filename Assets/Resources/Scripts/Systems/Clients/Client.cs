using Tcp4.Assets.Resources.Scripts.Managers;
using TMPro;
using UnityEngine;

namespace Tcp4.Assets.Resources.Scripts.Systems.Clients
{
    public class Client: MonoBehaviour
    {
        public string ID;
        public float stars;
        public float minimumQuality;
        public string nameClient;

        public GameObject modelClient;
        public  TextMeshProUGUI nameTmp;
        public Drink wantedProduct;
        public Sprite spriteClient;
        public UnityEngine.UI.Image ui_wantedProduct;
        public UnityEngine.UI.Image ui_timer;
        private float max_wait_time;
        private float wait_time;

        public void Setup(float _stars, float _minimum)
        {
            //setup basico
            stars = _stars;
            minimumQuality = _minimum;
            spriteClient = GameAssets.Instance.clientSprites[Random.Range(0, GameAssets.Instance.clientSprites.Count)];
            nameClient = GameAssets.Instance.clientNames[Random.Range(0, GameAssets.Instance.clientNames.Count)];
            modelClient = GameAssets.Instance.clientModels[Random.Range(0, GameAssets.Instance.clientModels.Count)];
            nameTmp.text = nameClient;
            max_wait_time = 30f - (stars * 2); 
            wait_time = max_wait_time;
            ui_timer.fillAmount = wait_time / max_wait_time;
            ID = GameAssets.GenerateID(5);

            //Criar model

            Debug.Log(modelClient);
            Instantiate(modelClient, gameObject.transform);

            ChooseDrink();
        }

        public void Update()
        {
            ui_timer.fillAmount = wait_time / max_wait_time;

            if(wait_time > 0f) wait_time -= Time.deltaTime;
            else { this.NotDelivered(); }
        }

        public void Delivered()
        {
            ShopManager.Instance.AddMoney(35); 
            ShopManager.Instance.AddStars(Random.Range(40f,50f) + stars);
            ClientManager.Instance.DeleteSpecificClient(this);
            Destroy(this.gameObject);
        }

        public void NotDelivered()
        {
            ShopManager.Instance.AddStars(-0.1f);
            ClientManager.Instance.DeleteSpecificClient(this);
            Destroy(this.gameObject);
        }

        public void ChooseDrink()
        {
            //Decidindo o pedido que eu quero!
            var currentMenu = UnlockManager.Instance.GetCurrentMenu();
            var rand = Random.Range(0, currentMenu.Count);

            Drink _drink = currentMenu[rand];

            if(_drink == null) 
            {
                //Se nao achou pega um expresso de cria mesmo
                _drink = currentMenu[0];
            }

            wantedProduct = _drink;
            ui_wantedProduct.sprite = wantedProduct.productImage; 
        }

        void Start()
        {
            transform.rotation = Quaternion.Euler(0, -90, 0);
        }

    }
}