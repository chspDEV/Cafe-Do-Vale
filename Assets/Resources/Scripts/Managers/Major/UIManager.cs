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


        #region Creation Management - COM PROTEÇÃO ANTI-SPAM

        private List<GameObject> CreationSlotInstances = new();
        private List<GameObject> IngredientsSlotInstances = new();

        [TabGroup("UI Interactions")][SerializeField] private Button createButton;

        // SISTEMA ANTI-SPAM ROBUSTO
        private static bool isProcessingInteraction = false;
        private static float lastInteractionTime = 0f;
        private const float INTERACTION_COOLDOWN = 0.3f; // Cooldown entre interações
        private static int activeCoroutines = 0;

        public void ControlCreationMenu(bool isActive)
        {
            if (isActive)
            {
                // PROTEÇÃO: Evita abrir menu se já está processando algo
                if (isProcessingInteraction)
                {
                    Debug.Log("Menu bloqueado - processando interação anterior");
                    return;
                }

                UpdateCreationView();
                UpdateIngredientsView();

                OpenMenu(creationMenu);
                StartCoroutine(SetupCreationNavigation());
            }
            else
            {
                CloseMenu(creationMenu);

                // CORREÇÃO: Limpa estado ao fechar menu
                ResetInteractionState();
            }
        }

        private void ResetInteractionState()
        {
            isProcessingInteraction = false;
            activeCoroutines = 0;
            Debug.Log("Estado de interação resetado");
        }

        private IEnumerator SetupCreationNavigation()
        {
            // PROTEÇÃO: Marca que está processando
            isProcessingInteraction = true;
            activeCoroutines++;

            yield return new WaitForSecondsRealtime(0.5f);

            // PROTEÇÃO: Verifica se ainda é válido continuar
            if (!creationMenu.activeInHierarchy)
            {
                activeCoroutines--;
                isProcessingInteraction = false;
                yield break;
            }

            ConfigureAllNavigation();
            SelectFirstCreationSlot();

            // PROTEÇÃO: Marca que terminou de processar
            activeCoroutines--;
            if (activeCoroutines <= 0)
            {
                isProcessingInteraction = false;
            }
        }

        public void SelectFirstCreationSlot()
        {
            // PROTEÇÃO: Evita múltiplas seleções simultâneas
            if (isProcessingInteraction && activeCoroutines > 1) return;

            StartCoroutine(SelectCreationSlotNextFrame());
        }

        public void RefreshCreationUIWithNavigation()
        {
            // PROTEÇÃO CRÍTICA: Evita refresh enquanto já está processando
            if (isProcessingInteraction)
            {
                Debug.Log("Refresh bloqueado - ainda processando interação anterior");
                return;
            }

            // PROTEÇÃO: Cooldown entre refreshes
            if (Time.unscaledTime - lastInteractionTime < INTERACTION_COOLDOWN)
            {
                Debug.Log($"Refresh bloqueado - cooldown ativo ({Time.unscaledTime - lastInteractionTime:F2}s)");
                return;
            }

            lastInteractionTime = Time.unscaledTime;

            UpdateCreationView();
            UpdateIngredientsView();

            StartCoroutine(DelayedCreationNavigationSetup());
        }

        private IEnumerator DelayedCreationNavigationSetup()
        {
            // PROTEÇÃO: Marca que está processando
            isProcessingInteraction = true;
            activeCoroutines++;

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // PROTEÇÃO: Verifica se ainda é válido continuar
            if (!creationMenu.activeInHierarchy)
            {
                activeCoroutines--;
                isProcessingInteraction = false;
                yield break;
            }

            ConfigureAllNavigation();
            SelectFirstCreationSlot();

            // PROTEÇÃO: Marca que terminou
            activeCoroutines--;
            if (activeCoroutines <= 0)
            {
                isProcessingInteraction = false;
            }
        }

        private IEnumerator SelectCreationSlotNextFrame()
        {
            activeCoroutines++;

            yield return new WaitForSecondsRealtime(0.1f);

            // PROTEÇÃO: Verifica se menu ainda está ativo
            if (!creationMenu.activeInHierarchy)
            {
                activeCoroutines--;
                yield break;
            }

            // PROTEÇÃO: Remove slots nulos de forma segura
            try
            {
                CreationSlotInstances.RemoveAll(go => go == null);

                if (CreationSlotInstances.Count > 0)
                {
                    var first = CreationSlotInstances[0].GetComponent<Selectable>();
                    if (first != null && first.gameObject.activeInHierarchy)
                    {
                        first.Select();
                        Debug.Log($"Selecionado primeiro slot: {first.name}");
                    }
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erro ao selecionar slot: {e.Message}");
                EventSystem.current.SetSelectedGameObject(null);
            }

            activeCoroutines--;
        }

        public void UpdateCreationView()
        {
            // PROTEÇÃO: Evita update durante processamento crítico
            if (isProcessingInteraction && activeCoroutines > 2)
            {
                Debug.Log("UpdateCreationView bloqueado - muitas operações simultâneas");
                return;
            }

            Inventory playerInventory = StorageManager.Instance.playerInventory;
            if (playerInventory == null) return;

            // PROTEÇÃO: Limpa de forma segura
            CleanCreationSlots();

            try
            {
                foreach (BaseProduct product in playerInventory.GetInventory())
                {
                    GameObject go = Instantiate(pfSlotCreation, creationSlotHolder);
                    CreationSlotInstances.Add(go);

                    SelectProduct selectProduct = go.GetComponent<SelectProduct>();
                    selectProduct.myProduct = product;
                    selectProduct.SetSlotType(SelectProduct.SlotType.InventorySlot);

                    go.GetComponent<DataSlot>().Setup(product.productImage, 1);

                    var button = go.GetComponent<Button>();
                    if (button != null)
                    {
                        button.interactable = true;
                    }
                }

                Debug.Log($"Criados {CreationSlotInstances.Count} slots de inventário");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erro ao atualizar creation view: {e.Message}");
                CleanCreationSlots();
            }
        }

        public void UpdateIngredientsView()
        {
            // PROTEÇÃO: Evita update durante processamento crítico
            if (isProcessingInteraction && activeCoroutines > 2)
            {
                Debug.Log("UpdateIngredientsView bloqueado - muitas operações simultâneas");
                return;
            }

            List<BaseProduct> ingredients = CreationManager.Instance.Ingredients;
            if (ingredients == null) return;

            // PROTEÇÃO: Limpa de forma segura
            CleanIngredientsSlots();

            try
            {
                foreach (var ingredient in ingredients)
                {
                    GameObject go = Instantiate(pfSlotCreationIngredient, ingredientSlotHolder);
                    IngredientsSlotInstances.Add(go);

                    SelectProduct selectProduct = go.GetComponent<SelectProduct>();
                    selectProduct.myProduct = ingredient;
                    selectProduct.SetSlotType(SelectProduct.SlotType.IngredientSlot);

                    go.GetComponent<DataSlot>().Setup(ingredient.productImage, ingredient.productName);

                    var button = go.GetComponent<Button>();
                    if (button != null)
                    {
                        button.interactable = true;
                    }
                }

                Debug.Log($"Criados {IngredientsSlotInstances.Count} slots de ingredientes");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erro ao atualizar ingredients view: {e.Message}");
                CleanIngredientsSlots();
            }
        }

        private void ConfigureAllNavigation()
        {
            // PROTEÇÃO: Só configura se não há muitas operações simultâneas
            if (activeCoroutines > 3)
            {
                Debug.Log("Configuração de navegação adiada - muitas operações simultâneas");
                return;
            }

            StartCoroutine(ConfigureNavigationAfterLayout());
        }

        private IEnumerator ConfigureNavigationAfterLayout()
        {
            activeCoroutines++;

            yield return new WaitForEndOfFrame();

            // PROTEÇÃO: Verifica se menu ainda está ativo
            if (!creationMenu.activeInHierarchy)
            {
                activeCoroutines--;
                yield break;
            }

            Debug.Log($"Configurando navegação APÓS layout - Slots: {CreationSlotInstances.Count}, Ingredientes: {IngredientsSlotInstances.Count}");

            try
            {
                ConfigureCreationSlotNavigation();
                ConfigureIngredientsNavigation();
                ConfigureCreateButtonNavigation();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erro ao configurar navegação: {e.Message}");
            }

            activeCoroutines--;
        }

        private void ConfigureCreationSlotNavigation()
        {
            const int slotsPerRow = 4;

            for (int i = 0; i < CreationSlotInstances.Count; i++)
            {
                // PROTEÇÃO: Verifica se o objeto ainda existe
                if (CreationSlotInstances[i] == null) continue;

                var selectable = CreationSlotInstances[i].GetComponent<Selectable>();
                if (selectable == null) continue;

                var navigation = new Navigation();
                navigation.mode = Navigation.Mode.Explicit;

                int row = i / slotsPerRow;
                int col = i % slotsPerRow;

                // Navegação para CIMA
                if (row > 0)
                {
                    int upIndex = (row - 1) * slotsPerRow + col;
                    if (upIndex >= 0 && upIndex < CreationSlotInstances.Count && CreationSlotInstances[upIndex] != null)
                    {
                        navigation.selectOnUp = CreationSlotInstances[upIndex].GetComponent<Selectable>();
                    }
                }

                // Navegação para BAIXO
                int nextRowIndex = (row + 1) * slotsPerRow + col;
                if (nextRowIndex < CreationSlotInstances.Count && CreationSlotInstances[nextRowIndex] != null)
                {
                    navigation.selectOnDown = CreationSlotInstances[nextRowIndex].GetComponent<Selectable>();
                }
                else
                {
                    if (IngredientsSlotInstances.Count > 0 && IngredientsSlotInstances[0] != null)
                    {
                        navigation.selectOnDown = IngredientsSlotInstances[0].GetComponent<Selectable>();
                    }
                    else if (createButton != null)
                    {
                        navigation.selectOnDown = createButton;
                    }
                }

                // Navegação para ESQUERDA
                if (col > 0 && i - 1 >= 0 && CreationSlotInstances[i - 1] != null)
                {
                    navigation.selectOnLeft = CreationSlotInstances[i - 1].GetComponent<Selectable>();
                }
                else if (IngredientsSlotInstances.Count > 0 && IngredientsSlotInstances[0] != null)
                {
                    navigation.selectOnLeft = IngredientsSlotInstances[0].GetComponent<Selectable>();
                }

                // Navegação para DIREITA
                if (col < slotsPerRow - 1 && i + 1 < CreationSlotInstances.Count && CreationSlotInstances[i + 1] != null)
                {
                    navigation.selectOnRight = CreationSlotInstances[i + 1].GetComponent<Selectable>();
                }

                selectable.navigation = navigation;
            }
        }

        private void ConfigureIngredientsNavigation()
        {
            for (int i = 0; i < IngredientsSlotInstances.Count; i++)
            {
                // PROTEÇÃO: Verifica se o objeto ainda existe
                if (IngredientsSlotInstances[i] == null) continue;

                var selectable = IngredientsSlotInstances[i].GetComponent<Selectable>();
                if (selectable == null) continue;

                var navigation = new Navigation();
                navigation.mode = Navigation.Mode.Explicit;

                // Navegação para CIMA
                if (i > 0 && IngredientsSlotInstances[i - 1] != null)
                {
                    navigation.selectOnUp = IngredientsSlotInstances[i - 1].GetComponent<Selectable>();
                }
                else if (CreationSlotInstances.Count > 0 && CreationSlotInstances[CreationSlotInstances.Count - 1] != null)
                {
                    navigation.selectOnUp = CreationSlotInstances[CreationSlotInstances.Count - 1].GetComponent<Selectable>();
                }

                // Navegação para BAIXO
                if (i < IngredientsSlotInstances.Count - 1 && IngredientsSlotInstances[i + 1] != null)
                {
                    navigation.selectOnDown = IngredientsSlotInstances[i + 1].GetComponent<Selectable>();
                }
                else if (createButton != null)
                {
                    navigation.selectOnDown = createButton;
                }

                // Navegação para DIREITA
                if (CreationSlotInstances.Count > 0 && CreationSlotInstances[0] != null)
                {
                    navigation.selectOnRight = CreationSlotInstances[0].GetComponent<Selectable>();
                }

                // Navegação para ESQUERDA
                if (CreationSlotInstances.Count > 0 && CreationSlotInstances[CreationSlotInstances.Count - 1] != null)
                {
                    navigation.selectOnLeft = CreationSlotInstances[CreationSlotInstances.Count - 1].GetComponent<Selectable>();
                }

                selectable.navigation = navigation;
            }
        }

        private void ConfigureCreateButtonNavigation()
        {
            if (createButton == null) return;

            var navigation = new Navigation();
            navigation.mode = Navigation.Mode.Explicit;

            // Navegação para CIMA
            if (IngredientsSlotInstances.Count > 0 && IngredientsSlotInstances[IngredientsSlotInstances.Count - 1] != null)
            {
                navigation.selectOnUp = IngredientsSlotInstances[IngredientsSlotInstances.Count - 1].GetComponent<Selectable>();
            }
            else if (CreationSlotInstances.Count > 0 && CreationSlotInstances[CreationSlotInstances.Count - 1] != null)
            {
                navigation.selectOnUp = CreationSlotInstances[CreationSlotInstances.Count - 1].GetComponent<Selectable>();
            }

            // Navegação lateral
            if (CreationSlotInstances.Count > 0)
            {
                if (CreationSlotInstances[CreationSlotInstances.Count - 1] != null)
                {
                    navigation.selectOnLeft = CreationSlotInstances[CreationSlotInstances.Count - 1].GetComponent<Selectable>();
                }
                if (CreationSlotInstances[0] != null)
                {
                    navigation.selectOnRight = CreationSlotInstances[0].GetComponent<Selectable>();
                }
            }

            createButton.navigation = navigation;
        }

        // MÉTODOS DE LIMPEZA COM PROTEÇÃO EXTRA
        public void CleanCreationSlots()
        {
            if (CreationSlotInstances == null || CreationSlotInstances.Count == 0) return;

            try
            {
                for (int i = CreationSlotInstances.Count - 1; i >= 0; i--)
                {
                    if (CreationSlotInstances[i] != null)
                    {
                        Destroy(CreationSlotInstances[i]);
                    }
                }
                CreationSlotInstances.Clear();
                Debug.Log("Slots de criação limpos com segurança");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erro ao limpar slots de criação: {e.Message}");
                CreationSlotInstances.Clear();
            }
        }

        public void CleanIngredientsSlots()
        {
            if (IngredientsSlotInstances == null || IngredientsSlotInstances.Count == 0) return;

            try
            {
                for (int i = IngredientsSlotInstances.Count - 1; i >= 0; i--)
                {
                    if (IngredientsSlotInstances[i] != null)
                    {
                        Destroy(IngredientsSlotInstances[i]);
                    }
                }
                IngredientsSlotInstances.Clear();
                Debug.Log("Slots de ingredientes limpos com segurança");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erro ao limpar slots de ingredientes: {e.Message}");
                IngredientsSlotInstances.Clear();
            }
        }

        // MÉTODO DE EMERGÊNCIA PARA RESETAR TUDO
        public void EmergencyResetCreationMenu()
        {
            Debug.LogWarning("RESET DE EMERGÊNCIA DO MENU DE CRIAÇÃO!");

            // Para todas as corrotinas
            StopAllCoroutines();

            // Reseta estado
            ResetInteractionState();

            // Limpa tudo
            CleanCreationSlots();
            CleanIngredientsSlots();

            // Fecha e reabre o menu
            if (creationMenu.activeInHierarchy)
            {
                CloseMenu(creationMenu);
                StartCoroutine(ReopenMenuAfterReset());
            }
        }

        private IEnumerator ReopenMenuAfterReset()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            ControlCreationMenu(true);
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
