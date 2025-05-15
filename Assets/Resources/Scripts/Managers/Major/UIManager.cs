using System.Collections.Generic;
using ComponentUtils.ComponentUtils.Scripts;
using Sirenix.OdinInspector;
using Tcp4.Assets.Resources.Scripts.Managers;
using Tcp4.Assets.Resources.Scripts.Systems.Clients;
using Tcp4.Assets.Resources.Scripts.Systems.Collect_Cook;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Tcp4
{
    public class UIManager : Singleton<UIManager>
    {
        #region UI Elements

        [TabGroup("Menus")]
        [TabGroup("Menus")] [SerializeField] private GameObject productionMenu;
        [TabGroup("Menus")] [SerializeField] private GameObject storageMenu;
        [TabGroup("Menus")] [SerializeField] private GameObject creationMenu;
        [TabGroup("Menus")] [SerializeField] private GameObject configMenu;
        [TabGroup("Menus")] [SerializeField] private GameObject seedShopMenu;

        [TabGroup("Prefabs")]
        [TabGroup("Prefabs")] public GameObject pfImageToFill;
        [TabGroup("Prefabs")] public GameObject pfImage;
        [TabGroup("Prefabs")] public GameObject pfSlotStorage;
        [TabGroup("Prefabs")] public GameObject pfSlotCreation, pfSlotCreationIngredient;
        [TabGroup("Prefabs")] public GameObject pfClientNotification;
        [TabGroup("Prefabs")] public GameObject pfProductionCard;

        [TabGroup("UI Containers")]
        [TabGroup("UI Containers")] public Transform storageSlotHolder;
        [TabGroup("UI Containers")] public Transform productionSlotHolder;
        [TabGroup("UI Containers")] public Transform creationSlotHolder, ingredientSlotHolder;
        [TabGroup("UI Containers")] public Transform notificationHolder;
        [TabGroup("UI Containers")] public Canvas hudCanvas;
        [TabGroup("UI Containers")] public Canvas worldCanvas;

        [TabGroup("UI Animations")]
        [TabGroup("UI Animations")] public AnimationExecute money;
        [TabGroup("UI Animations")] public AnimationExecute stars;

        #endregion

        #region UI Text & Images

        [TabGroup("UI Text")]
        [TabGroup("UI Text")] public TextMeshProUGUI moneyText;
        [TabGroup("UI Text")] public TextMeshProUGUI nameStorage;
        [TabGroup("UI Text")] public TextMeshProUGUI amountStorage;
        [TabGroup("UI Text")] public TextMeshProUGUI timeText;

        [TabGroup("UI Images")]
        public Image starImage;

        #endregion

        #region Storage Management

        private List<GameObject> StorageSlotInstances = new();

        public void ControlStorageMenu(bool isActive) => storageMenu.SetActive(isActive);

        public void CleanStorageSlots()
        {
            if (StorageSlotInstances == null || StorageSlotInstances.Count == 0) return;

            foreach (var go in StorageSlotInstances)
            {
                Destroy(go);
            }
            StorageSlotInstances.Clear();
        }

        public void UpdateStorageView()
        {
            StorageArea storage = StorageManager.Instance.GetStorageArea();
            if (storage == null) return;

            Inventory inventory = storage.inventory;

            nameStorage.text = storage.item.productName;
            amountStorage.text = $"{inventory.CountItem(storage.item)} / {inventory.GetLimit()}";

            CleanStorageSlots();

            foreach (BaseProduct _ in inventory.GetInventory())
            {
                GameObject go = Instantiate(pfSlotStorage, storageSlotHolder);
                StorageSlotInstances.Add(go);
                go.GetComponent<DataSlot>().Setup(storage.item.productImage, 1);
            }
        }

        #endregion

        #region Creation Management

        private List<GameObject> CreationSlotInstances = new();
        private List<GameObject> IngredientsSlotInstances = new();

        public void ControlCreationMenu(bool isActive) => creationMenu.SetActive(isActive);

        public void CleanCreationSlots()
        {
            if (CreationSlotInstances == null || CreationSlotInstances.Count == 0) return;

            foreach (var go in CreationSlotInstances)
            {
                Destroy(go);
            }
            CreationSlotInstances.Clear();
        }

        public void CleanIngredientsSlots()
        {
            if (IngredientsSlotInstances == null || IngredientsSlotInstances.Count == 0) return;

            foreach (var go in IngredientsSlotInstances)
            {
                Destroy(go);
            }
            IngredientsSlotInstances.Clear();
        }

        public void UpdateCreationView()
        {
            Inventory playerInventory = StorageManager.Instance.playerInventory;
            if (playerInventory == null) return;

            CleanCreationSlots();

            foreach (BaseProduct _ in playerInventory.GetInventory())
            {
                GameObject go = Instantiate(pfSlotCreation, creationSlotHolder);
                CreationSlotInstances.Add(go);
                SelectProduct s = go.GetComponent<SelectProduct>();
                s.myProduct = _;
                go.GetComponent<DataSlot>().Setup(s.myProduct.productImage, 1);
            }
        }

        public void UpdateIngredientsView()
        {
            List<BaseProduct> ingredients = CreationManager.Instance.Ingredients;
            if (ingredients == null) return;

            CleanIngredientsSlots();

            foreach (var _ in ingredients)
            {
                GameObject go = Instantiate(pfSlotCreationIngredient, ingredientSlotHolder);
                IngredientsSlotInstances.Add(go);
                SelectProduct s = go.GetComponent<SelectProduct>();
                s.myProduct = _;
                go.GetComponent<DataSlot>().Setup(s.myProduct.productImage, s.myProduct.productName);
            }
        }

        #endregion

        #region Production Management
        private List<GameObject> productionCardInstances = new();
        public List<GameObject> GetCardInstances() => productionCardInstances;
        public void CreateNewProductionCard(Production p)
        {
            if (p == null) return;

            GameObject go = Instantiate(pfProductionCard, productionSlotHolder);
            ProductionCard card = go.GetComponent<ProductionCard>();

            if (card == null)
            {
                Destroy(go);
                return;
            }

            card.myProduction = p;
            card.ConfigureVisuals();

            go.name = $"PRODUCAO: {p.product.productName}";
            productionCardInstances.Add(go);
        }

        public void ClearProductionCards()
        {
            foreach (var v in productionCardInstances)
            {
                Destroy(v);
            }

            productionCardInstances.Clear();
        }
        #endregion

        #region Seed Shop Management

        public void ControlSeedShop(bool isActive)
        { 
            seedShopMenu.SetActive(isActive);
        }

        #endregion
        #region Notifications

        public void NewClientNotification(Client clientSettings)
        {
            SoundManager.PlaySound(SoundType.interacao, 0.2f);
            GameObject go = Instantiate(pfClientNotification, notificationHolder);
            ClientNotification c = go.GetComponent<ClientNotification>();
            c.Setup(clientSettings.spriteClient, clientSettings.wantedProduct.productImage, clientSettings.stars);
        }

        public void OpenShopNotification() => Debug.Log("Loja aberta!");
        public void CloseShopNotification() => Debug.Log("Loja fechada!");

        #endregion

        #region Shop Management

        public void UpdateMoney()
        {
            moneyText.text = ShopManager.Instance.GetMoney().ToString();
            money.ExecuteAnimation("pop");
        }

        public void UpdateStars()
        {
            starImage.fillAmount = ShopManager.Instance.GetStars() / ShopManager.Instance.GetMaxStars();
            stars.ExecuteAnimation("pop");
        }

        #endregion

        #region Utility

        public void ControlProductionMenu(bool isActive) => productionMenu.SetActive(isActive);

        public void ControlConfigMenu()
        {
            SoundManager.PlaySound(SoundType.feedback);
            if (configMenu.activeSelf)
            {
                configMenu.SetActive(false);
                Time.timeScale = 1;
            }
            else
            {
                configMenu.SetActive(true);
                Time.timeScale = 0;
            }
        }

        public void VoltarMenu()
        {
            SceneManager.LoadScene("InitialMenu");
            SoundManager.PlaySound(SoundType.feedback);
        }

        private void PlaceInWorld(Transform worldObject, RectTransform uiElement, bool isWorldCanvas = true)
        {
            if (!isWorldCanvas)
            {
                Camera mainCamera = Camera.main;
                float camSize = mainCamera.orthographicSize;
                Vector2 canvasSize = new(worldCanvas.pixelRect.width, worldCanvas.pixelRect.height);

                float newX = worldObject.position.x * camSize * 2 / canvasSize.x;
                float newY = worldObject.position.y * camSize * 2 / canvasSize.y;
                float newZ = worldObject.position.z;

                uiElement.position = new Vector3(newX, newY, newZ);
            }
            else
            {
                uiElement.position = worldObject.position + new Vector3(0f, 2f, 0f);
            }
        }

        public ImageToFill PlaceFillImage(Transform pointToSpawn)
        {
            var obj = Instantiate(pfImageToFill, worldCanvas.gameObject.transform);
            var imageFill = obj.GetComponent<ImageToFill>();
            PlaceInWorld(pointToSpawn, imageFill.GetRectTransform());
            return imageFill;
        }

        public Image PlaceImage(Transform pointToSpawn)
        {
            var obj = Instantiate(pfImage, worldCanvas.gameObject.transform);
            var image = obj.GetComponent<Image>();
            PlaceInWorld(pointToSpawn, image.rectTransform);
            return image;
        }

        #endregion

        #region Time Management
        public void UpdateClock(float hour)
        {
            string _formatedHour = TimeManager.Instance.GetFormattedTime(hour);
            timeText.text = _formatedHour;
        }
        #endregion
    }
}
