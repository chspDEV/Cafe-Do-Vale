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
using System.Collections;
using UnityEngine.EventSystems;
using System.Linq;

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
        [TabGroup("UI Interactions")][SerializeField] public SetsUiElementToSelectOnInteraction seedShopInteraction;
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

        // anim do dinheiro modificando aos poucos
        private float moneyAnimMaxDuration = 2f;
        private float ticksPerSecond = 10f;
        private Coroutine updateMoneyCoroutine;

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

        public List<GameObject> CreationSlotInstances = new();
        public List<GameObject> IngredientsSlotInstances = new();

        [TabGroup("UI Interactions")][SerializeField] public Button createButton;

        public void ControlCreationMenu(bool isActive)
        {
            if (isActive)
            {
                // Atualiza e mostra os slots já existentes
                UpdateCreationView();
                UpdateIngredientsView();

                OpenMenu(creationMenu);

               
                StartCoroutine(SelectWithDelay());
            }
            else
            {
                // Apenas fecha o menu
                CloseMenu(creationMenu);
            }
        }

        IEnumerator SelectWithDelay()
        {
            yield return new WaitForSeconds(.3f);

            if (creationInteraction != null)
                creationInteraction.JumpToElement();
        }

        public void UpdateCreationView()
        {
            Inventory playerInventory = StorageManager.Instance.playerInventory;
            if (playerInventory == null) return;
            List<BaseProduct> inventoryItems = playerInventory.GetInventory();

            // Atualiza cada slot da UI
            for (int i = 0; i < CreationSlotInstances.Count; i++)
            {
                GameObject go = CreationSlotInstances[i];
                SelectProduct selectProduct = go.GetComponent<SelectProduct>();
                DataSlot dataSlot = go.GetComponent<DataSlot>();

                // VERIFICAÇÃO PRINCIPAL: Checa se o inventário tem um item nesta posição
                if (i < inventoryItems.Count)
                {
                    // Se tem, configura o slot com o item
                    BaseProduct product = inventoryItems[i];

                    selectProduct.myProduct = product;
                    dataSlot.Setup(product.productImage, 1);
                }
                else
                {
                    // Se NÃO tem, configura o slot como vazio
                    selectProduct.myProduct = null;
                    dataSlot.Setup(GameAssets.Instance.transparent, 0);
                }
            }
        }

        //

        public void UpdateIngredientsView()
        {
            List<BaseProduct> ingredients = CreationManager.Instance.Ingredients;
            if (ingredients == null) return;

            // Atualiza cada slot de ingrediente da UI
            for (int i = 0; i < IngredientsSlotInstances.Count; i++)
            {
                GameObject go = IngredientsSlotInstances[i];
                SelectProduct selectProduct = go.GetComponent<SelectProduct>();
                DataSlot dataSlot = go.GetComponent<DataSlot>();

                // Verifica se existe um ingrediente correspondente para este slot
                // e se esse ingrediente não é nulo.
                if (i < ingredients.Count && ingredients[i] != null)
                {
                    // Se existir, configura o slot com o ingrediente
                    BaseProduct product = ingredients[i];
                    selectProduct.myProduct = product;
                    dataSlot.Setup(product.productImage, 1);
                }
                else
                {
                    // Se não existir (ou for nulo), configura o slot como vazio
                    selectProduct.myProduct = null;
                    dataSlot.Setup(GameAssets.Instance.transparent, 0);
                }
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

                // CORREÇÃO: Adiciona delay antes de selecionar o elemento
                // para evitar seleção muito rápida
                StartCoroutine(DelayedSeedShopSelection());
            }
            else
            {
                CloseMenu(seedShopMenu);
            }
        }

        // MÉTODO NOVO: Adiciona delay na seleção do primeiro elemento
        private IEnumerator DelayedSeedShopSelection()
        {
            yield return new WaitForSecondsRealtime(0.3f);

            if (seedShopInteraction != null && seedShopMenu.activeInHierarchy)
                seedShopInteraction.JumpToElement();
        }

        #endregion

        #region Seed Inventory


        private List<GameObject> seedInventoryInstances = new();

        public void ControlSeedInventory(bool isActive)
        {
            if (isActive)
            {
                OpenMenu(bookMenu);
                OpenBookSection(3); // Inventário
                UpdateSeedInventoryView();
            }
            else
            {
                CloseMenu(bookMenu);
            }
                
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
            int.TryParse(moneyText.text, out int currentDisplayedMoney);

            int targetMoney = ShopManager.Instance.GetMoney();

            if (updateMoneyCoroutine != null)
            {
                StopCoroutine(updateMoneyCoroutine);
            }

            updateMoneyCoroutine = StartCoroutine(UpdateMoneyRoutine(currentDisplayedMoney, targetMoney));
        }

        #region Money Anim

        private IEnumerator UpdateMoneyRoutine(int startValue, int targetValue)
        {
            if (startValue == targetValue) yield break;

            float timeBetweenTicks = 1f / ticksPerSecond;
            float totalAmountToChange = targetValue - startValue;
            float totalTicks = moneyAnimMaxDuration * ticksPerSecond;

            int amountPerTick = Mathf.CeilToInt(Mathf.Abs(totalAmountToChange) / totalTicks);
            if (amountPerTick == 0) amountPerTick = 1;

            int direction = (totalAmountToChange > 0) ? 1 : -1;
            int currentDisplayValue = startValue;

            while (direction * (targetValue - currentDisplayValue) > 0)
            {
                currentDisplayValue += amountPerTick * direction;

                if (direction * (targetValue - currentDisplayValue) <= 0)
                {
                    currentDisplayValue = targetValue;
                }

                moneyText.text = currentDisplayValue.ToString();

                money.ExecuteAndRestartAnimation("pop_tick");

                yield return new WaitForSeconds(timeBetweenTicks);
            }
            moneyText.text = targetValue.ToString();
        }

        #endregion

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

            TimeManager.Instance.Freeze();
            DeactiveUi.ControlUi(HasMenuOpen());
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
            Debug.Log($"Fechando Ultimo menu aberto! Menu: {last.name}");
            DeactiveUi.ControlUi(HasMenuOpen());
            TimeManager.Instance.Unfreeze();
        }

        /// <summary>
        /// Fecha um menu específico (e remove do histórico).
        /// </summary>
        public void CloseMenu(GameObject menu)
        {
            if (menu == null) return;

            menu.SetActive(false);
            openedMenus.Remove(menu);
            DeactiveUi.ControlUi(HasMenuOpen());
            TimeManager.Instance.Unfreeze();
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

            if (ShopManager.Instance != null)
            {
                moneyText.text = ShopManager.Instance.GetMoney().ToString();
                starImage.fillAmount = ShopManager.Instance.GetStars() / ShopManager.Instance.GetMaxStars();
            }
        }

        private void Update()
        {
            if (gameAssets != null && gameAssets.playerMovement != null)
                gameAssets.playerMovement.ToggleMovement(openedMenus.Count <= 0 || openedMenus == null);
        }

        // para salvar os itens dos storages
        private void OnEnable()
        {
            if (StorageManager.Instance != null)
            {
                StorageManager.Instance.OnChangeStorage += UpdateStorageView;
                StorageManager.Instance.OnCleanStorage += CleanStorageSlots;
            }

            SaveManager.OnGameDataLoaded += OnGameLoadedUpdate;
        }


        private void OnDisable()
        {
            if (StorageManager.Instance != null)
            {
                StorageManager.Instance.OnChangeStorage -= UpdateStorageView;
                StorageManager.Instance.OnCleanStorage -= CleanStorageSlots;
            }

            SaveManager.OnGameDataLoaded -= OnGameLoadedUpdate;
        }


        private void OnGameLoadedUpdate()
        {
            if (storageMenu.activeInHierarchy)
            {
                UpdateStorageView();
            }
        }

    }
}
