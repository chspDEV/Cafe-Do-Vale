using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tcp4;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine.UI;

namespace Tcp4.Assets.Resources.Scripts.Systems.Areas
{
    public class RefinementArea : BaseInteractable
    {
        [TabGroup("Configurações", "Processo")]
        [SerializeField, MinValue(1)] private float refinementTime = 5f;

        [TabGroup("Configurações", "Processo")]
        [SerializeField, MinValue(1)] private int itemGetAmount = 1;

        [TabGroup("Configurações", "Processo")]
        [SerializeField, Range(1.1f, 2f)] private float spoilThreshold = 1.25f;

        [TabGroup("Configurações", "Referências")]
        [SerializeField, Required] private Transform pointToSpawn;
        [TabGroup("Configurações", "Referências")]
        [SerializeField] private AnimationExecute anim;
        [TabGroup("Configurações", "Referências")]
        [SerializeField] private RefinamentActivator activator;

        [TabGroup("Interface", "Sprites")]
        [SerializeField, PreviewField(50)] private Sprite insertProductSprite;
        [TabGroup("Interface", "Sprites")]
        [SerializeField, PreviewField(50)] private Sprite startRefinementSprite;
        [TabGroup("Interface", "Sprites")]
        [SerializeField, PreviewField(50)] private Sprite collectProductSprite;

        [TabGroup("Estado", "Debug")]
        [ShowInInspector, ReadOnly] private Queue<BaseProduct> processingQueue = new Queue<BaseProduct>();
        [TabGroup("Estado", "Debug")]
        [ShowInInspector, ReadOnly] private bool isRefining = false;
        [TabGroup("Estado", "Debug")]
        [ShowInInspector, ReadOnly] private bool isReady = false;
        [TabGroup("Estado", "Debug")]
        [ShowInInspector, ReadOnly] private bool isSpoiled = false;

        [TabGroup("Estado", "Debug")]
        [ShowInInspector, ReadOnly] private BaseProduct currentProduct;
        [TabGroup("Estado", "Debug")]
        [ShowInInspector, ReadOnly] private float maxTime;
        [TabGroup("Estado", "Debug")]
        [ShowInInspector, ReadOnly] private bool processingCurrentItem;

        [TabGroup("Configurações", "Processo")]
        [ValueDropdown("GetAllRecipes"), SerializeField]
        private RefinementRecipe selectedRecipe;

        private ImageToFill timeImage;
        private Inventory playerInventory;
        private float currentTime = 0f;
        private Coroutine collectRoutine;

        private bool isCountingDown = false;
        private float countdownTimer = 0f;

        public override void Start()
        {
            base.Start();
            timeImage = UIManager.Instance.PlaceFillImage(pointToSpawn);
            playerInventory = GameAssets.Instance.player.GetComponent<Inventory>();
            // Início: ativo para inserção
            EnableInteraction();
            UpdateInteractionUI();

            if (activator != null)
                activator.OnActive += StartRefinement;
        }

        public override void Update()
        {
            base.Update();

            if (processingCurrentItem)
            {
                currentTime += Time.deltaTime;
                timeImage.UpdateFill(currentTime);
                UpdateInteractionUI();

                if (currentTime >= refinementTime)
                    FinishRefinement();
            }
            else if (isCountingDown)
            {
                currentTime = 0f; // para UI de contagem secundária
                UpdateInteractionUI();

                countdownTimer += Time.deltaTime;
                timeImage.ChangeSprite(GameAssets.Instance.sprSpoilingWarning);
                timeImage.UpdateFill(countdownTimer);

                // Durante momento para estragar, permanece ativo
                if (!IsInteractable()) EnableInteraction();

                if (countdownTimer >= refinementTime * spoilThreshold)
                {
                    SpoiledProduct();
                    ResetCountdown();
                }
            }
        }

        public override void OnInteract()
        {
            if (!IsInteractable() || isRefining || isSpoiled) return;

            if (processingQueue.Count == 0)
                TryInsertProducts();
            else if (isReady)
                CollectProduct();

            UpdateInteractionUI();
        }

        private IEnumerable GetAllRecipes() =>
            RefinamentManager.Instance?.GetRecipes() ?? new List<RefinementRecipe>();

        private void TryInsertProducts()
        {
            if (playerInventory == null || selectedRecipe == null) return;

            int available = playerInventory.CountItem(selectedRecipe.inputProduct);
            if (available <= 0)
            {
                Debug.Log("Refinamento: sem produtos para inserir.");
                return;
            }

            int capacity = playerInventory.GetLimit() - processingQueue.Count;
            int amountToAdd = Mathf.Min(Mathf.Min(available, capacity), itemGetAmount);

            playerInventory.RemoveProduct(selectedRecipe.inputProduct, amountToAdd);
            for (int i = 0; i < amountToAdd; i++)
                processingQueue.Enqueue(selectedRecipe.inputProduct);

            
            // Após inserir: desativado
            DisableInteraction();
            activator.canInteract = true;
            activator.EnableInteraction();
            UpdateInteractionUI();
        }

        private void StartRefinement()
        {
            if (processingQueue.Count == 0 || processingCurrentItem) return;

            isRefining = true;
            processingCurrentItem = true;
            currentProduct = processingQueue.Peek();
            maxTime = refinementTime * spoilThreshold;
            currentTime = 0f;

            // Processo de refinamento: permanece desativado
            DisableInteraction();
            anim?.ExecuteAnimation("Refinar");
            timeImage.SetFillMethod(Image.FillMethod.Radial360);
            timeImage.ChangeSprite(startRefinementSprite);
        }

        private void FinishRefinement()
        {
            isReady = true;
            isRefining = false;
            processingCurrentItem = false;

            // Ao ficar pronto: ativo
            EnableInteraction();
            timeImage.SetFillMethod(Image.FillMethod.Vertical);
            timeImage.ChangeSprite(collectProductSprite);
            Debug.Log("Produto pronto para coleta.");

            collectRoutine = StartCoroutine(CollectCountdown());
            UpdateInteractionUI();
        }

        private void CompleteCurrentProcessing()
        {
            processingQueue.Dequeue();
            currentProduct = null;
            isReady = false;

            timeImage.SetFillMethod(Image.FillMethod.Vertical);
            if (processingQueue.Count > 0)
            {
                StartRefinement();
                return;
            }

            // Após coleta completa, volta ao estado ativo
            EnableInteraction();
            UpdateInteractionUI();
        }

        private IEnumerator CollectCountdown()
        {
            yield return new WaitForSeconds(3f);
            isCountingDown = true;
            countdownTimer = 0f;
        }

        private void ResetCountdown()
        {
            isCountingDown = false;
            countdownTimer = 0f;
        }

        private void CollectProduct()
        {
            if (!isReady) return;

            playerInventory.AddProduct(
                RefinamentManager.Instance.Refine(currentProduct),
                1
            );

            CompleteCurrentProcessing();
            if (collectRoutine != null)
                StopCoroutine(collectRoutine);

            activator.Reset();
            UpdateInteractionUI();
        }

        private void SpoiledProduct()
        {
            // Ao estragar: desativado
            DisableInteraction();
            processingCurrentItem = false;
            processingQueue.Clear();
            isRefining = false;
            isReady = false;
            isSpoiled = true;

            if (collectRoutine != null)
                StopCoroutine(collectRoutine);

            timeImage.UpdateFill(999f);
            StartCoroutine(ResetAfterDelay(3f));
            timeImage.SetFillMethod(Image.FillMethod.Vertical);
            UpdateInteractionUI();
        }

        private IEnumerator ResetAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            isSpoiled = false;
            // Após resetar: ativo
            EnableInteraction();
            activator.Reset();
            UpdateInteractionUI();
        }

        private void UpdateInteractionUI()
        {
            if (timeImage == null) return;

            bool isSpoiling = isRefining && currentTime > refinementTime && !isSpoiled;

            timeImage.ChangeSprite(
                isSpoiled ? GameAssets.Instance.sprError :
                isSpoiling ? GameAssets.Instance.sprSpoilingWarning :
                isReady ? GameAssets.Instance.ready :
                isRefining ? GameAssets.Instance.sprRefinamentWait :
                processingQueue.Count > 0 ? startRefinementSprite :
                insertProductSprite
            );

            float currentMax = isSpoiling ?
                (refinementTime * spoilThreshold) - refinementTime :
                refinementTime * (isRefining ? spoilThreshold : 1f);

            timeImage.SetupMaxTime(currentMax);
            timeImage.UpdateFill(isSpoiling ? currentTime - refinementTime : currentTime);
        }

        public void DecreaseRefinamentTime(float amount)
        {
            refinementTime -= amount;
            refinementTime = Mathf.Max(refinementTime, 0.1f);
        }
    }
}
