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


        #region Creation Management - VERSÃO CORRIGIDA COM SUBMIT

        private List<GameObject> CreationSlotInstances = new();
        private List<GameObject> IngredientsSlotInstances = new();

        [TabGroup("UI Interactions")][SerializeField] private Button createButton;

        public void ControlCreationMenu(bool isActive)
        {
            if (isActive)
            {
                UpdateCreationView();
                UpdateIngredientsView();
                
                OpenMenu(creationMenu);

                // CORREÇÃO: Mesmo padrão do SeedShop
                StartCoroutine(SetupCreationNavigation());
            }
            else
            {
                CloseMenu(creationMenu);
            }
        }

        private IEnumerator SetupCreationNavigation()
        {
            // CORREÇÃO: Mesmo delay do SeedShop (0.5s)
            yield return new WaitForSecondsRealtime(0.5f);

            ConfigureCreationSlotNavigation();

            // CORREÇÃO: Seleciona primeiro elemento como no SeedShop
            SelectFirstCreationSlot();
        }

        public void SelectFirstCreationSlot()
        {
            StartCoroutine(SelectCreationSlotNextFrame());
        }

        private IEnumerator SelectCreationSlotNextFrame()
        {
            yield return new WaitForSecondsRealtime(0.1f);

            CreationSlotInstances.RemoveAll(go => go == null);

            if (CreationSlotInstances.Count > 0)
            {
                var first = CreationSlotInstances[0].GetComponent<Selectable>();
                if (first != null)
                {
                    first.Select();
                }
            }
            else
            {
                // Fallback se não tiver slots
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void UpdateCreationView()
        {
            Inventory playerInventory = StorageManager.Instance.playerInventory;
            if (playerInventory == null) return;

            CleanCreationSlots();

            foreach (BaseProduct product in playerInventory.GetInventory())
            {
                GameObject go = Instantiate(pfSlotCreation, creationSlotHolder);
                CreationSlotInstances.Add(go);

                SelectProduct selectProduct = go.GetComponent<SelectProduct>();
                selectProduct.myProduct = product;
                selectProduct.SetSlotType(SelectProduct.SlotType.InventorySlot);

                go.GetComponent<DataSlot>().Setup(product.productImage, 1);

                // CORREÇÃO: Não desabilita o botão aqui, deixa habilitado
                var button = go.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = true; // ← MUDANÇA: deixa habilitado
                }
            }

            // CORREÇÃO: Não precisa mais do delay para habilitar
            // if (creationMenu.activeInHierarchy)
            // {
            //     StartCoroutine(EnableCreationButtonsWithDelay(0.5f));
            // }
        }

        public void UpdateIngredientsView()
        {
            List<BaseProduct> ingredients = CreationManager.Instance.Ingredients;
            if (ingredients == null) return;

            CleanIngredientsSlots();

            foreach (var ingredient in ingredients)
            {
                GameObject go = Instantiate(pfSlotCreationIngredient, ingredientSlotHolder);
                IngredientsSlotInstances.Add(go);

                SelectProduct selectProduct = go.GetComponent<SelectProduct>();
                selectProduct.myProduct = ingredient;
                selectProduct.SetSlotType(SelectProduct.SlotType.IngredientSlot);

                go.GetComponent<DataSlot>().Setup(ingredient.productImage, ingredient.productName);

                // CORREÇÃO: Deixa habilitado desde o início
                var button = go.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = true; // ← MUDANÇA: deixa habilitado
                }
            }

            // CORREÇÃO: Não precisa mais do delay
            // if (creationMenu.activeInHierarchy)
            // {
            //     StartCoroutine(EnableIngredientButtonsWithDelay(0.5f));
            // }
        }

        private IEnumerator EnableIngredientButtonsWithDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            foreach (var go in IngredientsSlotInstances)
            {
                if (go != null)
                {
                    var button = go.GetComponent<Button>();
                    if (button != null)
                    {
                        button.interactable = true;
                    }
                }
            }
        }

        // NOVO: Método baseado no EnableButtonsWithDelay do SeedShop
        private IEnumerator EnableCreationButtonsWithDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            foreach (var go in CreationSlotInstances)
            {
                if (go != null)
                {
                    var button = go.GetComponent<Button>();
                    if (button != null)
                    {
                        button.interactable = true;
                    }
                }
            }
        }

        private IEnumerator DelayedNavigationSetup()
        {
            yield return new WaitForSecondsRealtime(0.2f);
            ConfigureCreationSlotNavigation();
        }

        private void ConfigureCreationSlotNavigation()
        {
            if (CreationSlotInstances.Count == 0) return;

            // Organiza os slots em uma grid 4x4
            int slotsPerRow = 4;

            for (int i = 0; i < CreationSlotInstances.Count; i++)
            {
                var currentSlot = CreationSlotInstances[i];
                var selectable = currentSlot.GetComponent<Selectable>();

                if (selectable == null) continue;

                var navigation = new Navigation();
                navigation.mode = Navigation.Mode.Explicit;

                int row = i / slotsPerRow;
                int col = i % slotsPerRow;

                // Navegação horizontal (esquerda/direita)
                if (col > 0) // Não é o primeiro da linha
                {
                    navigation.selectOnLeft = CreationSlotInstances[i - 1].GetComponent<Selectable>();
                }
                else if (IngredientsSlotInstances.Count > row && IngredientsSlotInstances[row] != null)
                {
                    // É o primeiro da linha - vai para ingredientes se disponíveis
                    navigation.selectOnLeft = IngredientsSlotInstances[row].GetComponent<Selectable>();
                }

                if (col < slotsPerRow - 1 && i + 1 < CreationSlotInstances.Count)
                {
                    navigation.selectOnRight = CreationSlotInstances[i + 1].GetComponent<Selectable>();
                }
                else if (IngredientsSlotInstances.Count > row && IngredientsSlotInstances[row] != null)
                {
                    // É o último da linha - vai para ingredientes se disponíveis
                    navigation.selectOnRight = IngredientsSlotInstances[row].GetComponent<Selectable>();
                }

                // Navegação vertical (cima/baixo)
                if (row > 0) // Não é a primeira linha
                {
                    int upIndex = (row - 1) * slotsPerRow + col;
                    if (upIndex >= 0 && upIndex < CreationSlotInstances.Count)
                    {
                        navigation.selectOnUp = CreationSlotInstances[upIndex].GetComponent<Selectable>();
                    }
                }

                if (row < (CreationSlotInstances.Count - 1) / slotsPerRow) // Não é a última linha
                {
                    int downIndex = (row + 1) * slotsPerRow + col;
                    if (downIndex < CreationSlotInstances.Count)
                    {
                        navigation.selectOnDown = CreationSlotInstances[downIndex].GetComponent<Selectable>();
                    }
                }
                else if (col >= 1 && col <= 2 && createButton != null) // Última linha, colunas centrais
                {
                    navigation.selectOnDown = createButton;
                }

                selectable.navigation = navigation;
            }

            // Configura navegação dos ingredientes
            ConfigureIngredientsNavigation();
            ConfigureCreateButtonNavigation();
        }

        private void ConfigureIngredientsNavigation()
        {
            for (int i = 0; i < IngredientsSlotInstances.Count; i++)
            {
                var currentSlot = IngredientsSlotInstances[i];
                var selectable = currentSlot?.GetComponent<Selectable>();

                if (selectable == null) continue;

                var navigation = new Navigation();
                navigation.mode = Navigation.Mode.Explicit;

                // Ingredientes navegam verticalmente entre si
                if (i > 0 && IngredientsSlotInstances[i - 1] != null)
                {
                    navigation.selectOnUp = IngredientsSlotInstances[i - 1].GetComponent<Selectable>();
                }

                if (i < IngredientsSlotInstances.Count - 1 && IngredientsSlotInstances[i + 1] != null)
                {
                    navigation.selectOnDown = IngredientsSlotInstances[i + 1].GetComponent<Selectable>();
                }
                else if (i == IngredientsSlotInstances.Count - 1 && createButton != null)
                {
                    navigation.selectOnDown = createButton;
                }

                // Navegação horizontal - vai para os slots da linha correspondente
                int slotsPerRow = 4;
                if (i < CreationSlotInstances.Count / slotsPerRow)
                {
                    int leftmostSlotIndex = i * slotsPerRow;
                    int rightmostSlotIndex = Mathf.Min((i * slotsPerRow) + 3, CreationSlotInstances.Count - 1);

                    if (leftmostSlotIndex < CreationSlotInstances.Count)
                    {
                        navigation.selectOnLeft = CreationSlotInstances[leftmostSlotIndex].GetComponent<Selectable>();
                        navigation.selectOnRight = CreationSlotInstances[rightmostSlotIndex].GetComponent<Selectable>();
                    }
                }

                selectable.navigation = navigation;
            }
        }

        private void ConfigureCreateButtonNavigation()
        {
            if (createButton == null) return;

            var navigation = new Navigation();
            navigation.mode = Navigation.Mode.Explicit;

            // Botão Criar vai para cima nos ingredientes ou slots centrais da última linha
            if (IngredientsSlotInstances.Count > 0)
            {
                navigation.selectOnUp = IngredientsSlotInstances[IngredientsSlotInstances.Count - 1].GetComponent<Selectable>();
            }
            else if (CreationSlotInstances.Count > 0)
            {
                int lastRowStart = (CreationSlotInstances.Count - 1) / 4 * 4;
                int centralSlot = Mathf.Min(lastRowStart + 1, CreationSlotInstances.Count - 1);
                navigation.selectOnUp = CreationSlotInstances[centralSlot].GetComponent<Selectable>();
            }

            createButton.navigation = navigation;
        }

        public void CleanCreationSlots()
        {
            if (CreationSlotInstances == null || CreationSlotInstances.Count == 0) return;

            foreach (var go in CreationSlotInstances)
            {
                if (go != null) Destroy(go);
            }
            CreationSlotInstances.Clear();
        }

        public void CleanIngredientsSlots()
        {
            if (IngredientsSlotInstances == null || IngredientsSlotInstances.Count == 0) return;

            foreach (var go in IngredientsSlotInstances)
            {
                if (go != null) Destroy(go);
            }
            IngredientsSlotInstances.Clear();
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
