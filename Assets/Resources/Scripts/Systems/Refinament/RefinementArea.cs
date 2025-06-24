using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tcp4;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine.UI;
using GameResources.Project.Scripts.Utilities.Audio;

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
        private GameAssets gameAssets;

        private bool isCountingDown = false;
        private float countdownTimer = 0f;

        public override void Start()
        {
            base.Start();
            timeImage = UIManager.Instance.PlaceFillImage(pointToSpawn);
            gameAssets = GameAssets.Instance;
            if (gameAssets != null && gameAssets.player != null)
            {
                playerInventory = gameAssets.player.GetComponent<Inventory>();
            }
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
                if (timeImage != null) timeImage.UpdateFill(currentTime);
                UpdateInteractionUI();

                if (currentTime >= refinementTime)
                    FinishRefinement();
            }
            else if (isCountingDown)
            {
                currentTime = 0f;
                UpdateInteractionUI();

                countdownTimer += Time.deltaTime;
                if (timeImage != null)
                {
                    timeImage.ChangeSprite(GameAssets.Instance.sprSpoilingWarning);
                    timeImage.UpdateFill(countdownTimer);
                }

                if (!IsInteractable()) EnableInteraction();

                if (countdownTimer >= refinementTime * spoilThreshold)
                {
                    SpoiledProduct();
                }
            }
        }

        public override void OnInteract()
        {
            if (!IsInteractable() || isRefining || isSpoiled)
            {
                return;
            } 

            if (processingQueue.Count == 0 && !isReady)
                TryInsertProducts();
            else if (isReady)
                CollectProduct();

        }

        private IEnumerable GetAllRecipes() =>
            RefinamentManager.Instance?.GetRecipes() ?? new List<RefinementRecipe>();

        private void TryInsertProducts()
        {
            if (playerInventory == null || selectedRecipe == 
                null) return;

            int available = playerInventory.CountItem(selectedRecipe.inputProduct);
            if (available <= 0)
            {
                //Fazendo o request de sfx
                SoundEventArgs sfx1Args = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "erro", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                    VolumeScale = .6f // Escala de volume (opcional, padrão é 1f)
                };
                SoundEvent.RequestSound(sfx1Args);

                Debug.Log("Refinamento: sem produtos para inserir.");
                return;
            }

            //Fazendo o request de sfx
            SoundEventArgs sfxArgs = new()
            {
                Category = SoundEventArgs.SoundCategory.SFX,
                AudioID = "colocando", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                Position = transform.position, // Posição para o som 3D
                VolumeScale = .6f // Escala de volume (opcional, padrão é 1f)
            };
            SoundEvent.RequestSound(sfxArgs);

            int capacity = playerInventory.GetLimit() - processingQueue.Count;
            int amountToAdd = Mathf.Min(Mathf.Min(available, capacity), itemGetAmount);

            if (amountToAdd <= 0) return;

            playerInventory.RemoveProduct(selectedRecipe.inputProduct, amountToAdd);
            for (int i = 0; i < amountToAdd; i++)
                processingQueue.Enqueue(selectedRecipe.inputProduct);

            DisableInteraction();
            if (activator != null)
            {
                activator.canInteract = true;
                activator.EnableInteraction();
            }
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

            DisableInteraction();
            if (anim != null) anim.ExecuteAnimation("Refinar");

            if (timeImage != null) timeImage.SetFillMethod(Image.FillMethod.Radial360);

            UpdateInteractionUI();
        }

        private void FinishRefinement()
        {
            isReady = true;
            isRefining = false;
            processingCurrentItem = false;

            EnableInteraction();
            if (timeImage != null)
            {
                timeImage.SetFillMethod(Image.FillMethod.Vertical);
                timeImage.ChangeSprite(collectProductSprite);
            }
            Debug.Log("Produto pronto para coleta.");
            //Fazendo o request de sfx
            SoundEventArgs sfxArgs = new()
            {
                Category = SoundEventArgs.SoundCategory.SFX,
                AudioID = "concluido", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                Position = transform.position, // Posição para o som 3D
                VolumeScale = .8f, // Escala de volume (opcional, padrão é 1f)

            };
            SoundEvent.RequestSound(sfxArgs);

            if (collectRoutine != null) StopCoroutine(collectRoutine);
            collectRoutine = StartCoroutine(CollectCountdown());
            UpdateInteractionUI();
            //
        }

        private void CompleteCurrentProcessing()
        {
            if (processingQueue.Count > 0) processingQueue.Dequeue();
            currentProduct = null;
            isReady = false;

            if (timeImage != null) timeImage.SetFillMethod(Image.FillMethod.Vertical);

            if (processingQueue.Count > 0)
            {
                StartRefinement();
                return;
            }

            if (activator != null) activator.Reset();
            EnableInteraction();
            UpdateInteractionUI();
        }

        private IEnumerator CollectCountdown()
        {
            yield return new WaitForSeconds(5f);
            isCountingDown = true;
            countdownTimer = 0f;
            UpdateInteractionUI();
        }

        private void ResetCountdown()
        {
            isCountingDown = false;
            countdownTimer = 0f;
        }

        private void CollectProduct()
        {
            if (!isReady) return;

            if (collectRoutine != null)
            {
                StopCoroutine(collectRoutine);
                collectRoutine = null;
            }

            if (isCountingDown)
            {
                ResetCountdown();
            }

            if (playerInventory != null && RefinamentManager.Instance != null && currentProduct != null)
            {
                playerInventory.AddProduct(
                    RefinamentManager.Instance.Refine(currentProduct),
                    1
                );

                //Fazendo o request de sfx
                SoundEventArgs ostArgs = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "coletar", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                    VolumeScale = .7f // Escala de volume (opcional, padrão é 1f)
                };

                SoundEvent.RequestSound(ostArgs);
            }

            CompleteCurrentProcessing();
        }

        private void SpoiledProduct()
        {
            DisableInteraction();
            processingCurrentItem = false;
            processingQueue.Clear();
            currentProduct = null;

            isRefining = false;
            isReady = false;
            isSpoiled = true;

            if (collectRoutine != null)
            {
                StopCoroutine(collectRoutine);
                collectRoutine = null;
            }
            ResetCountdown();

            StartCoroutine(ResetAfterDelay(.5f));
            if (timeImage != null) timeImage.SetFillMethod(Image.FillMethod.Vertical);
            UpdateInteractionUI();
        }

        private IEnumerator ResetAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            isSpoiled = false;
            if (timeImage != null)
            {
                timeImage.SetupMaxTime(1f);
                timeImage.UpdateFill(1f);
            }
            EnableInteraction();
            if (activator != null) activator.Reset();
            UpdateInteractionUI();
        }

        private void UpdateInteractionUI()
        {
            if (timeImage == null || gameAssets == null) return;

            bool isSpoilingOriginalDefinition = isRefining && currentTime > refinementTime && !isSpoiled && !isReady;

            Sprite spriteToSet;
            float maxTimeToSet;
            float fillAmountToSet;

            if (isSpoiled)
            {
                spriteToSet = gameAssets.sprError;
                maxTimeToSet = 1f;
                fillAmountToSet = 1f;
            }
            else if (isCountingDown && isReady)
            {
                spriteToSet = gameAssets.sprSpoilingWarning;
                maxTimeToSet = refinementTime * spoilThreshold;
                fillAmountToSet = countdownTimer;
            }
            else if (isReady)
            {
                spriteToSet = gameAssets.ready;
                maxTimeToSet = 1f;
                fillAmountToSet = 1f;
            }
            else if (isSpoilingOriginalDefinition)
            {
                spriteToSet = gameAssets.sprSpoilingWarning;
                maxTimeToSet = refinementTime * spoilThreshold;
                fillAmountToSet = currentTime - refinementTime;
            }
            else if (isRefining)
            {
                spriteToSet = gameAssets.sprRefinamentWait;
                maxTimeToSet = refinementTime;
                fillAmountToSet = currentTime;
            }
            else if (processingQueue.Count > 0)
            {
                spriteToSet = startRefinementSprite;
                maxTimeToSet = refinementTime;
                fillAmountToSet = 0f;
            }
            else
            {
                spriteToSet = insertProductSprite;
                maxTimeToSet = 1f;
                fillAmountToSet = 1f;
            }

            timeImage.ChangeSprite(spriteToSet);
            timeImage.SetupMaxTime(maxTimeToSet);
            timeImage.UpdateFill(fillAmountToSet);
        }

        public void DecreaseRefinamentTime(float amount)
        {
            refinementTime -= amount;
            refinementTime = Mathf.Max(refinementTime, 0.1f);
        }
    }
}