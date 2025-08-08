using Tcp4;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectProduct : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler
{
    public BaseProduct myProduct;

    public enum SlotType
    {
        InventorySlot,
        IngredientSlot
    }

    [SerializeField] private SlotType slotType = SlotType.InventorySlot;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    private Button button;
    private bool isInitialized = false;
    private static float lastInteractionTime;
    private const float INTERACTION_DELAY = 0.3f;

    // CORREÇÃO: Variáveis de proteção baseadas no SeedShop
    private static float timeCreationMenuOpened;
    private const float PROTECTION_TIME = 0.8f; // Mesmo tempo do SeedShop

    public static void NotifyCreationMenuOpened()
    {
        timeCreationMenuOpened = Time.unscaledTime;
    }

    private void Start()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        if (isInitialized) return;

        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            if (slotType == SlotType.InventorySlot)
            {
                button.onClick.AddListener(Select);
            }
            else
            {
                button.onClick.AddListener(Unselect);
            }
        }

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }

        isInitialized = true;
    }

    // Implementa ISubmitHandler para capturar Submit do controle
    public void OnSubmit(BaseEventData eventData)
    {
        Debug.Log($"OnSubmit chamado para {gameObject.name}, SlotType: {slotType}");

        if (slotType == SlotType.InventorySlot)
        {
            Select();
        }
        else
        {
            Unselect();
        }

        eventData.Use();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = selectedColor;
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
    }

    public void Select()
    {
        // CORREÇÃO 1: Proteção baseada no SeedShop
        if (Time.unscaledTime - timeCreationMenuOpened < PROTECTION_TIME)
        {
            Debug.Log($"Clique bloqueado! Tempo desde abertura do menu: {Time.unscaledTime - timeCreationMenuOpened:F2}s");
            return;
        }

        if (Time.unscaledTime - lastInteractionTime < INTERACTION_DELAY)
        {
            Debug.Log("Interação bloqueada - muito rápida!");
            return;
        }

        if (!isInitialized) InitializeComponent();

        if (myProduct == null)
        {
            Debug.LogWarning($"Produto não definido no slot {gameObject.name}");
            return;
        }

        if (!CreationManager.Instance.CanAdd())
        {
            Debug.Log("Não é possível adicionar mais ingredientes!");
            return;
        }

        if (slotType != SlotType.InventorySlot)
        {
            Debug.LogWarning($"Tentando usar Select() em um slot do tipo {slotType}");
            return;
        }

        // CORREÇÃO 2: Verifica se o botão está realmente interativo (como no SeedShop)
        if (button != null && !button.interactable)
        {
            Debug.Log("Botão não está interativo!");
            return;
        }

        Debug.Log($"Selecionando produto: {myProduct.productName}");
        lastInteractionTime = Time.unscaledTime;

        CreationManager.Instance.SelectProduct(myProduct);
        CreationManager.Instance.NotifyInventoryUpdate();

        if (button != null)
        {
            button.interactable = false;
            Invoke(nameof(ReactivateButton), INTERACTION_DELAY);
        }
    }

    public void Unselect()
    {
        // CORREÇÃO 3: Mesma proteção para Unselect
        if (Time.unscaledTime - timeCreationMenuOpened < PROTECTION_TIME)
        {
            Debug.Log($"Clique bloqueado! Tempo desde abertura do menu: {Time.unscaledTime - timeCreationMenuOpened:F2}s");
            return;
        }

        if (Time.unscaledTime - lastInteractionTime < INTERACTION_DELAY)
        {
            Debug.Log("Interação bloqueada - muito rápida!");
            return;
        }

        if (!isInitialized) InitializeComponent();

        if (myProduct == null)
        {
            Debug.LogWarning($"Produto não definido no slot {gameObject.name}");
            return;
        }

        if (slotType != SlotType.IngredientSlot)
        {
            Debug.LogWarning($"Tentando usar Unselect() em um slot do tipo {slotType}");
            return;
        }

        if (button != null && !button.interactable)
        {
            Debug.Log("Botão não está interativo!");
            return;
        }

        Debug.Log($"Removendo ingrediente: {myProduct.productName}");
        lastInteractionTime = Time.unscaledTime;

        CreationManager.Instance.UnselectProduct(myProduct);
        CreationManager.Instance.NotifyInventoryUpdate();

        if (button != null)
        {
            button.interactable = false;
            Invoke(nameof(ReactivateButton), INTERACTION_DELAY);
        }
    }

    private void ReactivateButton()
    {
        if (button != null)
        {
            button.interactable = true;
        }
    }

    public void SetSlotType(SlotType type)
    {
        slotType = type;
        if (isInitialized && button != null)
        {
            button.onClick.RemoveAllListeners();
            if (slotType == SlotType.InventorySlot)
            {
                button.onClick.AddListener(Select);
            }
            else
            {
                button.onClick.AddListener(Unselect);
            }
        }
    }

    public void SetColors(Color normal, Color selected)
    {
        normalColor = normal;
        selectedColor = selected;
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
    }
}