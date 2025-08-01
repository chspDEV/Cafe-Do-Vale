using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tcp4.Assets.Resources.Scripts.Managers;
using Tcp4.Assets.Resources.Scripts.Systems.Clients;
using Tcp4.Assets.Resources.Scripts.Systems.Collect_Cook;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ChristinaCreatesGames.UI;
using GameResources.Project.Scripts.Utilities.Audio;

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

        [TabGroup("Menus")] [SerializeField] private GameObject bookMenu;
        [TabGroup("Menus")] [SerializeField] private GameObject seedInventoryMenu;
        [TabGroup("Menus")] [SerializeField] private GameObject recipeMenu;
        [TabGroup("Menus")] [SerializeField] private GameObject mapMenu;
        [TabGroup("Menus")] [SerializeField] private UI_Book uiBook;

        private List<GameObject> openedMenus = new List<GameObject>();

        [TabGroup("UI Interactions")][SerializeField] private SetsUiElementToSelectOnInteraction mapInteraction;
        [TabGroup("UI Interactions")][SerializeField] private SetsUiElementToSelectOnInteraction recipeInteraction;
        [TabGroup("UI Interactions")][SerializeField] private SetsUiElementToSelectOnInteraction pauseInteraction;
        [TabGroup("UI Interactions")][SerializeField] private SetsUiElementToSelectOnInteraction seedInventoryInteraction;
        [TabGroup("UI Interactions")][SerializeField] private SetsUiElementToSelectOnInteraction seedShopInteraction;
        [TabGroup("UI Interactions")][SerializeField] private SetsUiElementToSelectOnInteraction notificationInteraction;


        [TabGroup("UI Interactions")][SerializeField] private SetsUiElementToSelectOnInteraction productionInteraction;
        [TabGroup("UI Interactions")][SerializeField] private SetsUiElementToSelectOnInteraction creationInteraction;


        [TabGroup("UI Interactions")][SerializeField] private SetsUiElementToSelectOnInteraction storageInteraction;

        [TabGroup("Prefabs")]
        [TabGroup("Prefabs")] public GameObject pfImageToFill;
        [TabGroup("Prefabs")] public GameObject pfImage;
        [TabGroup("Prefabs")] public GameObject pfSlotStorage;
        [TabGroup("Prefabs")] public GameObject pfSlotCreation, pfSlotCreationIngredient;
        [TabGroup("Prefabs")] public GameObject pfClientNotification;
        [TabGroup("Prefabs")] public GameObject pfProductionCard;
        [TabGroup("Prefabs")] public GameObject pfUpgradeText;
        [TabGroup("Prefabs")] public GameObject pfSeedInventoryCard;

        [TabGroup("UI Containers")]
        [TabGroup("UI Containers")] public Transform storageSlotHolder;
        [TabGroup("UI Containers")] public Transform productionSlotHolder;
        [TabGroup("UI Containers")] public Transform creationSlotHolder, ingredientSlotHolder;
        [TabGroup("UI Containers")] public Transform notificationHolder;
        [TabGroup("UI Containers")] public Transform seedInventorySlotHolder;
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

        GameAssets gameAssets;

        #endregion

        #region Storage Management

        private List<GameObject> StorageSlotInstances = new();

        public void ControlStorageMenu(bool isActive)
        {
            if (isActive)
            {
                OpenMenu(storageMenu);

                if (storageInteraction != null)
                    storageInteraction.JumpToElement();
            }
            else
            {
                CloseMenu(storageMenu);
            }
        }

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

        public void ControlCreationMenu(bool isActive)
        {
            if (isActive)
            {
                OpenMenu(creationMenu);

                if (creationInteraction != null)
                    creationInteraction.JumpToElement();
            }
            else
            {
                CloseMenu(creationMenu);
            }

        }
        

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

            go.name = $"PRODUCAO: {p.outputProduct.productName}";
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
            if (isActive)
            {
                OpenMenu(seedShopMenu);

                if (seedShopInteraction != null)
                    seedShopInteraction.JumpToElement();
            }
            else
            {
                CloseMenu(seedShopMenu);
            }
        }

        #endregion
        #region Seed Inventory


        private List<GameObject> seedInventoryInstances = new();

        public void ControlSeedInventory(bool isActive)
        {
            if (isActive)
            {
                OpenBookSection(3); // Inventário
                UpdateSeedInventoryView();
            }
            else
                bookMenu.SetActive(false);
        }

        public void UpdateSeedInventoryView()
        {
            ClearSeedInventory();

            foreach (var pair in SeedManager.Instance.GetInventory)
            {
                if (pair.Value <= 0) continue;

                GameObject go = Instantiate(pfSeedInventoryCard, seedInventorySlotHolder);
                var card = go.GetComponent<SeedInventoryCard>();
                card.Configure(pair.Key, pair.Value);
                seedInventoryInstances.Add(go);
            }
        }

        private void ClearSeedInventory()
        {
            foreach (var obj in seedInventoryInstances)
            {
                Destroy(obj);
            }
            seedInventoryInstances.Clear();
        }

        #endregion
        #region Notifications

        //public void NewClientNotification(Client clientSettings)
        //{
        //    //Fazendo o request de sfx
        //    /*SoundEventArgs sfxArgs = new()
        //    {
        //        Category = SoundEventArgs.SoundCategory.SFX,
        //        AudioID = "interacao", // O ID do seu SFX (sem "sfx_" e em minúsculas)
        //        Position = gameAssets.player.transform.position, // Posição para o som 3D
        //        VolumeScale = .3f // Escala de volume (opcional, padrão é 1f)
        //    };
        //    SoundEvent.RequestSound(sfxArgs);*/

        //    //REFAZER O SISTEMA DE NOTIFICAO
        //    //GameObject go = Instantiate(pfClientNotification, notificationHolder);
        //    //ClientNotification c = go.GetComponent<ClientNotification>();
        //    //c.Setup(clientSettings.wantedProduct, clientSettings.wantedProduct.sprite, clientSettings.);
        //}

        public void OpenShopNotification() => NotificationManager.Instance.Show("Loja Aberta!", "Atenda os clientes.");  //Debug.Log("Loja aberta!");
        public void CloseShopNotification() => NotificationManager.Instance.Show("Loja Fechada!", ""); // Debug.Log("Loja fechada!");

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

        public void GoToMainMenu()
        {
            SceneManager.LoadScene("InitialMenu");
        }

        public void ControlProductionMenu(bool isActive)
        {

            if (isActive)
            {
                OpenMenu(productionMenu);

                if (productionInteraction != null)
                    productionInteraction.JumpToElement();

                //Fazendo o request de sfx
                SoundEventArgs sfxArgs = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "feedback", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                    Position = gameAssets.player.transform.position, // Posição para o som 3D
                    VolumeScale = 1.0f // Escala de volume (opcional, padrão é 1f)
                };
                SoundEvent.RequestSound(sfxArgs);
            }
            else
            {
                CloseMenu(productionMenu);
            }

            
        }

        public void ControlMap(bool isActive)
        {

            if (isActive)
            {
                OpenMenu(bookMenu);
                OpenBookSection(1);

                //Fazendo o request de sfx
                SoundEventArgs sfxArgs = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "feedback", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                    Position = gameAssets.player.transform.position, // Posição para o som 3D
                    VolumeScale = 1.0f // Escala de volume (opcional, padrão é 1f)
                };
                SoundEvent.RequestSound(sfxArgs);
            }
            else
            {
                CloseMenu(bookMenu);
            }

        }

        public void ControlRecipeMenu(bool isActive)
        {

            if (isActive)
            {
                OpenMenu(bookMenu);
                OpenBookSection(4);

                //Fazendo o request de sfx
                SoundEventArgs sfxArgs = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "feedback", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                    Position = gameAssets.player.transform.position, // Posição para o som 3D
                    VolumeScale = 1.0f // Escala de volume (opcional, padrão é 1f)
                };
                SoundEvent.RequestSound(sfxArgs);
            }
            else
            {
                CloseMenu(bookMenu);
            }

        }

        public void ControlConfigMenu(bool isActive)
        {
            if (isActive)
            {
                OpenMenu(bookMenu);
                OpenBookSection(0);

                //Fazendo o request de sfx
                SoundEventArgs sfxArgs = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "feedback", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                    Position = gameAssets.player.transform.position, // Posição para o som 3D
                    VolumeScale = 1.0f // Escala de volume (opcional, padrão é 1f)
                };
                SoundEvent.RequestSound(sfxArgs);
            }
            else
            {
                Debug.Log("fechei menu pause");
                CloseMenu(bookMenu);
            }
        }

        // para abrir no tabButton correto
        public void OpenBookSection(int tabIndex)
        {
            if (bookMenu != null && !bookMenu.activeSelf)
                bookMenu.SetActive(true);

            uiBook.ForceShowTab(tabIndex);
        }

        public void VoltarMenu()
        {
            SceneManager.LoadScene("InitialMenu");
            //Fazendo o request de sfx
            SoundEventArgs sfxArgs = new()
            {
                Category = SoundEventArgs.SoundCategory.SFX,
                AudioID = "feedback", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                Position = transform.position, // Posição para o som 3D
                VolumeScale = 1.0f // Escala de volume (opcional, padrão é 1f)
            };
            SoundEvent.RequestSound(sfxArgs);
        }

        /// <summary>
        /// Abre um menu e registra na lista de histórico.
        /// </summary>
        public void OpenMenu(GameObject menu)
        {
            if (menu == null) return;

            // Se já estiver aberto, removemos a ocorrência anterior
            openedMenus.Remove(menu);

            // Abrimos e registramos
            menu.SetActive(true);
            openedMenus.Add(menu);
        }

        /// <summary>
        /// Fecha o menu mais recentemente aberto.
        /// </summary>
        public void CloseLastMenu()
        {
            if (openedMenus.Count == 0) return;

            // Pega o último, fecha e remove da lista
            var last = openedMenus[openedMenus.Count - 1];
            last.SetActive(false);
            openedMenus.RemoveAt(openedMenus.Count - 1);
        }

        /// <summary>
        /// Fecha um menu específico (e remove do histórico).
        /// </summary>
        public void CloseMenu(GameObject menu)
        {
            if (menu == null) return;

            menu.SetActive(false);
            openedMenus.Remove(menu);
        }

        public bool HasMenuOpen()
        { 
            return openedMenus.Count > 0;
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

        public TextToProgress PlaceTextProgress(Transform pointToSpawn, float maxValue, float currentValue = 0f)
        {
            var obj = Instantiate(pfUpgradeText, worldCanvas.gameObject.transform);
            var textProgress = obj.GetComponent<TextToProgress>();

            textProgress.Setup(maxValue, currentValue);

            PlaceInWorld(pointToSpawn, textProgress.GetRectTransform());

            return textProgress;
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

        private void Start()
        {
            gameAssets = GameAssets.Instance;
        }

        private void Update()
        {
            if (gameAssets != null && gameAssets.playerMovement != null)
            gameAssets.playerMovement.ToggleMovement(openedMenus.Count <= 0 || openedMenus == null);
        }
    }
}
