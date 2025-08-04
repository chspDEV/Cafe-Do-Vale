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
        //[SerializeField] private float timeToGive = 1.3f;
        [SerializeField] private int amount;
        [SerializeField] private float timeToOpenInterface;
        [SerializeField] private List<Production> canProduce;

        [Space(10)]

        [Header("View")]
        [SerializeField] private Transform pointToSpawn;
        [SerializeField] private GameObject currentModel;

        private ImageToFill timeImage;
        private Inventory playerInventory;
        private ObjectPool objectPools;
        private float currentTime;
        private bool isAbleToGive;
        private bool isGrown;
        private bool hasChoosedProduction;

        TimeManager timeManager;
        [Space(10)]

        [Header("Minigame")]
        [SerializeField] MinigameTrigger minigameTrigger;

        [Header("System Integration")]
        [SerializeField] public int areaID;


        public void RegisterAreaID()
        {
            // Adicione esta linha para registrar a estação:
            if (WorkerManager.Instance != null && GameAssets.Instance != null)
            {
                areaID = GameAssets.Instance.GenerateAreaID();
                WorkerManager.Instance.RegisterProductionStation(areaID, this);
            }
        }

        private void CreateWorkerTask()
        {
            WorkerManager.Instance.CreateHarvestTask(this.areaID, this.production.outputProduct);
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

            RegisterAreaID();
        }

        private void OnDisable()
        {
            ProductionManager.Instance.OnChooseProduction -= SelectProduction;
            timeManager.OnTimeMultiplierChanged -= ReSetupMaxTime;

            // Garantir que se desinscreveu se estava inscrito
            if (minigameTrigger != null && minigameTrigger.minigameToStart != null)
                minigameTrigger.minigameToStart.OnGetReward -= this.ResetGrowthCycle;
        }

        public override void OnInteract()
        {

            playerInventory = GameAssets.Instance.player.GetComponent<Inventory>();
            if (playerInventory == null) { Debug.Log("Inventario do Jogador nulo!"); return; }


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

        public override void OnFocus()
        {
            if (!isGrown && hasChoosedProduction) return;
            base.OnFocus();
        }

        public override void OnLostFocus()
        {
            base.OnLostFocus();

            CloseProductionMenu();
            playerInventory = null;
        }

        private void InitializeObjectPools()
        {
            objectPools = new ObjectPool(pointToSpawn);
            objectPools.AddPool(production.models);

            //Fazendo o request de sfx
            SoundEventArgs sfxArgs = new()
            {
                Category = SoundEventArgs.SoundCategory.SFX,
                AudioID = "plantando", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                Position = transform.position, // Posição para o som 3D
                VolumeScale = 0.9f // Escala de volume (opcional, padrão é 1f)
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
            if (timeImage != null && timeManager != null)
                timeImage.SetupMaxTime(production.timeToGrow);
        }

        private void UpdateCurrentTime()
        {
            if (production != null && !isGrown)
            {
                //currentTime = Mathf.Clamp(currentTime, 0, production.timeToGrow);
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

            if (!isAbleToGive && currentTime < production.timeToGrow * timeManager.timeMultiplier)
            {
                timeImage.ChangeSprite(GameAssets.Instance.sprProductionWait);
            }
            else if (isAbleToGive)
            {
                timeImage.ChangeSprite(GameAssets.Instance.ready);
            }
        }


        IEnumerator OpenProductionCoroutine()
        {
            yield return new WaitForSeconds(timeToOpenInterface);

            if(playerInventory != null)
            OpenProductionMenu();
        }
        

        private void OpenProductionMenu()
        {
            var productionManager = ProductionManager.Instance;
            productionManager.Clean();
            productionManager.SetupNewReference(this);
            productionManager.ReloadCards(canProduce);
            UIManager.Instance.ControlProductionMenu(true);
        }

        private void CloseProductionMenu()
        {
            ProductionManager.Instance.Clean();
            UIManager.Instance.ControlProductionMenu(false);
        }

        private void SelectProduction()
        {
            var productionManager = ProductionManager.Instance;
            if (productionManager.GetCurrentReference() != this) return;

            production = productionManager.GetNewProduction();

            if (production == null) return;

            currentTime = 0;
            timeImage.SetupMaxTime(production.timeToGrow);
            CloseProductionMenu();
            hasChoosedProduction = true;
            productionManager.OnChooseProduction -= SelectProduction;
            productionManager.Clean();

            if (production.models.Length > 0)
            {
                InitializeObjectPools();
            }

            StartCoroutine(GrowthCycle());
        }



        private IEnumerator GrowthCycle()
        {
            // Verificações de segurança
            if (production == null || timeManager == null)
            {
                Debug.LogError($"Production or TimeManager is null in {gameObject.name}");
                yield break;
            }

            OnLostFocus();
            var models = production.models;

            // Registrar tempo inicial
            float startTime = timeManager.CurrentHour;
            float targetTime = startTime + production.timeToGrow;

            // Verificar se há modelos para mostrar
            bool hasModels = models != null && models.Length > 0;

            while (timeManager.CurrentHour < targetTime)
            {
                // Calcular progresso baseado no tempo global do jogo
                float elapsedTime = timeManager.CurrentHour - startTime;
                currentTime = Mathf.Clamp(elapsedTime, 0, production.timeToGrow);
                UpdateCurrentTime();

                // Atualizar modelos visuais (se houver)
                if (hasModels)
                {
                    int modelIndex = Mathf.FloorToInt((currentTime / production.timeToGrow) * models.Length);
                    modelIndex = Mathf.Clamp(modelIndex, 0, models.Length - 1);

                    if (currentModel == null || !currentModel.name.StartsWith(models[modelIndex].name))
                    {
                        if (currentModel != null && objectPools != null)
                        {
                            objectPools.Return(currentModel);
                        }

                        if (objectPools != null)
                        {
                            currentModel = objectPools.Get(models[modelIndex]);
                            currentModel.transform.SetPositionAndRotation(
                                pointToSpawn.position,
                                models[modelIndex].transform.rotation
                            );
                        }
                    }
                }

                yield return null;
            }

            // Finalização do crescimento
            EnableInteraction();
            CreateWorkerTask();
            isGrown = true;
            isAbleToGive = true;
        }


        private void HarvestProduct()
        {
            if (isAbleToGive && isGrown && playerInventory != null && playerInventory.CanStorage())
            {
                // Verificação adicional antes de usar o minigame
                if (minigameTrigger == null || minigameTrigger.minigameToStart == null)
                {
                    Debug.LogError($"MinigameTrigger or minigame is null in {gameObject.name}");
                    return;
                }

                if (production == null || production.outputProduct == null)
                {
                    Debug.LogError($"Production or outputProduct is null in {gameObject.name}");
                    return;
                }

                //Minigame
                // INSCREVER-SE APENAS QUANDO INICIAR O MINIGAME
                minigameTrigger.minigameToStart.OnGetReward += this.ResetGrowthCycle;

                minigameTrigger.minigameToStart.SetupReward(production.outputProduct);
                minigameTrigger.TriggerMinigame();
                DisableInteraction();

                InteractionManager.Instance.UpdateLastId(production.outputProduct.productName);
            }
        }

        public void HarvestProductFromWorker()
        {
            if (isAbleToGive && isGrown)
            {
                if (production.outputProduct == null)
                {
                    Debug.LogError("Produto de saída é nulo!");
                    return;
                }

                ResetGrowthCycle();
                CreateWorkerTask(); 
                Debug.Log($"[Worker] Colheu: {production.outputProduct.productName}");
            }
        }


        void ResetGrowthCycle()
        {
            // DESINSCREVER-SE IMEDIATAMENTE APÓS RECEBER O CALLBACK
            if (minigameTrigger != null && minigameTrigger.minigameToStart != null)
                minigameTrigger.minigameToStart.OnGetReward -= this.ResetGrowthCycle;


            // Verificações de segurança antes de reiniciar o ciclo
            if (production == null)
            {
                Debug.LogError($"Production is null in {gameObject.name}. Cannot reset growth cycle.");
                return;
            }

            if (timeManager == null)
            {
                Debug.LogError($"TimeManager is null in {gameObject.name}. Cannot reset growth cycle.");
                return;
            }

            if (objectPools == null && production.models.Length > 0)
            {
                Debug.LogWarning($"ObjectPools is null in {gameObject.name}. Reinitializing...");
                InitializeObjectPools();
            }

            currentTime = 0;
            isAbleToGive = false;
            isGrown = false;
            StartCoroutine(GrowthCycle());
        }
    }
}
