using GameResources.Project.Scripts.Utilities.Audio;
using System.Collections;
using System.Collections.Generic;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;

namespace Tcp4
{
    public class ProductionArea : BaseInteractable
    {
        [Header("Setup")]
        [SerializeField] private Production production;
        [SerializeField] private int amount;
        [SerializeField] private float timeToOpenInterface;
        [SerializeField] private List<Production> canProduce;

        [Space(10)]
        [Header("View")]
        [SerializeField] private Transform pointToSpawn;
        [SerializeField] private GameObject currentModel;

        [Header("Minigame")]
        [SerializeField] MinigameTrigger minigameTrigger;

        [Header("System Integration")]
        [SerializeField] public int areaID;

        private ImageToFill timeImage;
        private Inventory playerInventory;
        private ObjectPool objectPools;
        private float currentTime;
        private bool isAbleToGive;
        private bool isGrown;
        private bool hasChoosedProduction;
        private bool hasReachedMaturity;
        private TimeManager timeManager;

        public event System.Action<ProductionArea, BaseProduct> OnProductionComplete;

        private bool isTaskedForHarvest = false;

        private void CreateWorkerTask()
        {
            if (isTaskedForHarvest || !HasHarvestableProduct()) return;

            if (WorkerManager.Instance != null && production != null && production.outputProduct != null)
            {
                isGrown = true;
                isAbleToGive = true;
                isTaskedForHarvest = true;
                WorkerManager.Instance.CreateHarvestTask(this.areaID, this.production.outputProduct);
            }
        }

        public override void Start()
        {
            base.Start();
            interactable_id = "productionArea";
            hasChoosedProduction = false;
            ProductionManager.Instance.OnChooseProduction += SelectProduction;
            TimeManager.Instance.OnTimeMultiplierChanged += ReSetupMaxTime;
            timeImage = UIManager.Instance.PlaceFillImage(pointToSpawn);
            timeManager = TimeManager.Instance;

            if (WorkerManager.Instance != null && GameAssets.Instance != null)
            {
                areaID = GameAssets.Instance.GenerateAreaID();
                WorkerManager.Instance.RegisterProductionStation(areaID, this);
            }
        }

        private void OnDisable()
        {
            if (ProductionManager.Instance != null)
            {
                ProductionManager.Instance.OnChooseProduction -= SelectProduction;
            }
            if (timeManager != null)
            {
                timeManager.OnTimeMultiplierChanged -= ReSetupMaxTime;
            }

            if (minigameTrigger != null && minigameTrigger.minigameToStart != null)
                minigameTrigger.minigameToStart.OnGetReward -= HandleSuccessfulHarvest;
        }

        public override void OnInteract()
        {
            playerInventory = GameAssets.Instance.player.GetComponent<Inventory>();
            if (playerInventory == null) return;

            if (!hasChoosedProduction)
            {
                StartCoroutine(OpenProductionCoroutine());
                InteractionManager.Instance.UpdateLastId(interactable_id);
            }
            else
            {
                HarvestProduct();
            }
        }

        private void InitializeObjectPools()
        {
            objectPools = new ObjectPool(pointToSpawn);
            objectPools.AddPool(production.models);

            if (production.postHarvestModel != null)
            {
                objectPools.AddPool(new GameObject[] { production.postHarvestModel });
            }

            SoundEventArgs sfxArgs = new()
            {
                Category = SoundEventArgs.SoundCategory.SFX,
                AudioID = "plantando",
                Position = transform.position,
                VolumeScale = 0.9f
            };
            SoundEvent.RequestSound(sfxArgs);
        }

        public override void Update()
        {
            base.Update();
            SpritesLogic();
        }

        public void ReSetupMaxTime()
        {
            if (timeImage != null && timeManager != null && production != null)
            {
                float maxTime = hasReachedMaturity ? production.timeToRegrow : production.timeToGrow;
                timeImage.SetupMaxTime(maxTime);
            }
        }

        private void UpdateCurrentTime()
        {
            if (production != null && !isGrown)
            {
                timeImage.UpdateFill(currentTime);
            }
        }

        private void SpritesLogic()
        {
            if (production == null)
            {
                timeImage.ChangeSprite(GameAssets.Instance.transparent);
                return;
            }

            if (isTaskedForHarvest && isAbleToGive)
            {
                timeImage.ChangeSprite(GameAssets.Instance.sprProductionWait);
            }
            else if (isAbleToGive)
            {
                if (production.readyIcon != null)
                {
                    timeImage.ChangeSprite(production.readyIcon);
                }
                else
                {
                    timeImage.ChangeSprite(GameAssets.Instance.ready);
                }
            }
            else if (hasChoosedProduction)
            {
                timeImage.ChangeSprite(GameAssets.Instance.sprProductionWait);
            }
        }

        private void SelectProduction()
        {
            var productionManager = ProductionManager.Instance;
            if (productionManager.GetCurrentReference() != this) return;

            production = productionManager.GetNewProduction();
            if (production == null) return;

            currentTime = 0;
            hasChoosedProduction = true;
            hasReachedMaturity = false; // ÚNICO LUGAR QUE ISSO FICA FALSE

            ReSetupMaxTime();
            CloseProductionMenu();

            productionManager.OnChooseProduction -= SelectProduction;
            productionManager.Clean();

            InitializeObjectPools();
            StartCoroutine(GrowthCycle());
        }

        private IEnumerator GrowthCycle()
        {
            if (production == null || timeManager == null) yield break;

            OnLostFocus();
            float startTime = timeManager.CurrentHour;

            // SE AINDA NÃO É MADURA, FAZ O PRIMEIRO CRESCIMENTO
            if (!hasReachedMaturity)
            {
                float targetTime = startTime + production.timeToGrow;
                while (timeManager.CurrentHour < targetTime)
                {
                    float elapsedTime = timeManager.CurrentHour - startTime;
                    currentTime = Mathf.Clamp(elapsedTime, 0, production.timeToGrow);
                    UpdateCurrentTime();

                    float progress = currentTime / production.timeToGrow;
                    UpdateModelForGrowth(progress);

                    yield return null;
                }
                hasReachedMaturity = true;
            }
            // SE JÁ É MADURA, APENAS ESPERA O NOVO TEMPO
            else
            {
                float targetTime = startTime + production.timeToRegrow;
                ReSetupMaxTime();
                while (timeManager.CurrentHour < targetTime)
                {
                    float elapsedTime = timeManager.CurrentHour - startTime;
                    currentTime = Mathf.Clamp(elapsedTime, 0, production.timeToRegrow);
                    UpdateCurrentTime();
                    yield return null;
                }
            }

            // AO FINAL DE QUALQUER CICLO, GARANTE O MODELO FINAL E O ESTADO DE "PRONTO"
            OnGrowthComplete();
        }

        private void OnGrowthComplete()
        {
            SetModelToHarvestable(); // Alterna para o modelo com frutos
            EnableInteraction();
            isGrown = true;
            isAbleToGive = true;

            if (production.outputProduct != null)
            {
                OnProductionComplete?.Invoke(this, production.outputProduct);
                CreateWorkerTask();
            }
        }

        private void HarvestProduct()
        {
            if (isAbleToGive && isGrown && playerInventory != null && playerInventory.CanStorage())
            {
                minigameTrigger.minigameToStart.OnGetReward += this.HandleSuccessfulHarvest;
                minigameTrigger.minigameToStart.SetupReward(production.outputProduct);
                GameAssets.Instance.SetupLastIconMinigamePlants(production.outputProduct.productImage);
                minigameTrigger.TriggerMinigame();
                DisableInteraction();
                InteractionManager.Instance.UpdateLastId(production.outputProduct.productName);
            }
            else
            {
                SoundEventArgs sfxArgs = new() { Category = SoundEventArgs.SoundCategory.SFX, AudioID = "erro", VolumeScale = 0.5f };
                SoundEvent.RequestSound(sfxArgs);
                NotificationManager.Instance.Show("Inventario Cheio!", "Sem espaços livres.", production.outputProduct.productImage);
            }
        }

        public bool HarvestProductFromWorker()
        {
            if (!HasHarvestableProduct() || !isTaskedForHarvest) return false;

            HandleSuccessfulHarvest();
            return true;
        }

        private void HandleSuccessfulHarvest()
        {
            if (minigameTrigger != null && minigameTrigger.minigameToStart != null)
                minigameTrigger.minigameToStart.OnGetReward -= this.HandleSuccessfulHarvest;

            if (isTaskedForHarvest) ReleaseReservation();

            // Reseta o estado
            isAbleToGive = false;
            isGrown = false;
            currentTime = 0;

            // Alterna para o modelo sem frutos
            SetModelToPostHarvest();

            // Inicia o novo ciclo de crescimento
            StartCoroutine(GrowthCycle());
        }

        private void UpdateModelForGrowth(float progress)
        {
            if (production?.models == null || production.models.Length == 0) return;

            int modelIndex = Mathf.FloorToInt(progress * production.models.Length);
            modelIndex = Mathf.Clamp(modelIndex, 0, production.models.Length - 1);

            var targetModel = production.models[modelIndex];
            if (currentModel == null || !currentModel.name.StartsWith(targetModel.name))
            {
                if (currentModel != null) objectPools.Return(currentModel);
                currentModel = objectPools.Get(targetModel);
                currentModel.transform.SetPositionAndRotation(pointToSpawn.position, targetModel.transform.rotation);
            }
        }

        private void SetModelToHarvestable()
        {
            if (production?.models == null || production.models.Length == 0) return;

            var finalModel = production.models[production.models.Length - 1];
            if (currentModel != null) objectPools.Return(currentModel);
            currentModel = objectPools.Get(finalModel);
            currentModel.transform.SetPositionAndRotation(pointToSpawn.position, finalModel.transform.rotation);
        }

        private void SetModelToPostHarvest()
        {
            if (production?.postHarvestModel == null)
            {
                if (currentModel != null) objectPools.Return(currentModel);
                currentModel = null;
                return;
            }

            if (currentModel != null) objectPools.Return(currentModel);
            currentModel = objectPools.Get(production.postHarvestModel);
            currentModel.transform.SetPositionAndRotation(pointToSpawn.position, production.postHarvestModel.transform.rotation);
        }

        public override void OnFocus() { if (isTaskedForHarvest) { base.OnLostFocus(); return; } if (!isGrown && hasChoosedProduction) return; base.OnFocus(); }
        public override void OnLostFocus() { base.OnLostFocus(); CloseProductionMenu(); playerInventory = null; }
        IEnumerator OpenProductionCoroutine() { yield return new WaitForSeconds(timeToOpenInterface); if (playerInventory != null) OpenProductionMenu(); }
        private void OpenProductionMenu() { var pm = ProductionManager.Instance; pm.Clean(); pm.SetupNewReference(this); pm.ReloadCards(canProduce); UIManager.Instance.ControlProductionMenu(true); }
        private void CloseProductionMenu() { if (ProductionManager.Instance != null) { ProductionManager.Instance.Clean(); UIManager.Instance.ControlProductionMenu(false); } }
        public void ReserveForWorker() { if (!HasHarvestableProduct()) return; isTaskedForHarvest = true; }
        public void ReleaseReservation() { isTaskedForHarvest = false; }
        public bool IsReservedForWorker() { return isTaskedForHarvest; }
        public bool HasHarvestableProduct() { return isAbleToGive && isGrown; }
        public BaseProduct GetCurrentProduct() { if (HasHarvestableProduct()) return production.outputProduct; return null; }
    }
}