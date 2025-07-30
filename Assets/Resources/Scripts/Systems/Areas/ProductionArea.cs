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

        public override void Start()
        {
            base.Start();

            interactable_id = "productionArea";
            hasChoosedProduction = false;
            ProductionManager.Instance.OnChooseProduction += SelectProduction;
            TimeManager.Instance.OnTimeMultiplierChanged += ReSetupMaxTime;

            timeImage = UIManager.Instance.PlaceFillImage(pointToSpawn);
            timeManager = TimeManager.Instance;

            if(minigameTrigger != null)
                minigameTrigger.minigameToStart.OnGetReward += this.ResetGrowthCycle;
        }

        private void OnDisable()
        {
            ProductionManager.Instance.OnChooseProduction -= SelectProduction;
            timeManager.OnTimeMultiplierChanged -= ReSetupMaxTime;

            if (minigameTrigger != null)
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
            if(timeImage != null && timeManager != null)
                timeImage.SetupMaxTime(production.timeToGrow * timeManager.timeMultiplier);
        }

        private void UpdateCurrentTime()
        {
            if (production != null && !isGrown)
            {
                currentTime = Mathf.Clamp(currentTime, 0, production.timeToGrow * timeManager.timeMultiplier);
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
            timeImage.SetupMaxTime(production.timeToGrow * timeManager.timeMultiplier);
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
            if (production == null)
            {
                Debug.LogError($"Production is null in {gameObject.name}. Stopping growth cycle.");
                yield break;
            }

            if (timeManager == null)
            {
                Debug.LogError($"TimeManager is null in {gameObject.name}. Stopping growth cycle.");
                yield break;
            }

            OnLostFocus();
            var models = production.models;

            // Verificação adicional para models
            if (models == null || models.Length == 0)
            {
                Debug.LogWarning($"No models found for production in {gameObject.name}. Skipping model spawning.");

                // Ainda assim, execute o tempo de crescimento
                float _timeToGrow = production.timeToGrow * timeManager.timeMultiplier;
                float elapsedTime = 0;

                while (elapsedTime < _timeToGrow)
                {
                    elapsedTime += Time.deltaTime;
                    currentTime += Time.deltaTime;
                    UpdateCurrentTime();
                    yield return null;
                }

                isGrown = true;
                isAbleToGive = true;
                yield break;
            }

            var timeToGrow = production.timeToGrow * timeManager.timeMultiplier;
            int modelIndex = 0;

            while (modelIndex < models.Length)
            {
                if (currentModel != null && objectPools != null)
                {
                    objectPools.Return(currentModel);
                }

                if (objectPools != null)
                {
                    currentModel = objectPools.Get(models[modelIndex]);

                    Vector3 normalizedPosition = new(pointToSpawn.position.x,
                    pointToSpawn.position.y, pointToSpawn.position.z);

                    currentModel.transform.SetPositionAndRotation(normalizedPosition, models[modelIndex].transform.rotation);
                    Debug.Log($"Modelo atual: {currentModel.name} // Posicao: {normalizedPosition}");
                }

                float modelGrowTime = timeToGrow / models.Length;
                float elapsedTime = 0;

                while (elapsedTime < modelGrowTime)
                {
                    elapsedTime += Time.deltaTime;
                    currentTime += Time.deltaTime;
                    UpdateCurrentTime();
                    yield return null;
                }

                modelIndex++;
            }

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
                minigameTrigger.minigameToStart.SetupReward(production.outputProduct);
                minigameTrigger.TriggerMinigame();
                InteractionManager.Instance.UpdateLastId(production.outputProduct.productName);
            }
        }

        void ResetGrowthCycle()
        {
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
