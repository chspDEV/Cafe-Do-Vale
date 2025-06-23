using GameResources.Project.Scripts.Utilities.Audio;
using System.Collections;
using System.Collections.Generic;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;

namespace Tcp4
{
    public class CollectArea : BaseInteractable
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
        private const string PlayerTag = "Player";

        public override void Start()
        {
            base.Start();
            hasChoosedProduction = false;
            ProductionManager.Instance.OnChooseProduction += SelectProduction;
            timeImage = UIManager.Instance.PlaceFillImage(pointToSpawn);
        }

     

    
        public override void OnInteract()
        {
            
            playerInventory = GameAssets.Instance.player.GetComponent<Inventory>();
            if (playerInventory == null) { Debug.Log("Inventario do Jogador nulo!"); return; }

            Debug.Log("interagiiiiiiiiiiiiiii");

            if (!hasChoosedProduction)
            {
                StartCoroutine(OpenProductionCoroutine());
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

        private void UpdateCurrentTime()
        {
            if (production != null && !isGrown)
            {
                currentTime = Mathf.Clamp(currentTime, 0, production.timeToGrow);
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

            if (!isAbleToGive && currentTime < production.timeToGrow)
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
            OnLostFocus();
            var models = production.models;
            var timeToGrow = production.timeToGrow;
            int modelIndex = 0;

            while (modelIndex < models.Length)
            {
                if (currentModel != null)
                {
                    objectPools.Return(currentModel);
                }

                currentModel = objectPools.Get(models[modelIndex]);

                Vector3 normalizedPosition = new(pointToSpawn.position.x,
                pointToSpawn.position.y, pointToSpawn.position.z);

                currentModel.transform.SetPositionAndRotation(normalizedPosition, models[modelIndex].transform.rotation);
                Debug.Log($"Modelo atual: {currentModel.name} // Posicao: {normalizedPosition}");

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
                //Fazendo o request de sfx
                SoundEventArgs sfxArgs = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "coletar", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                    VolumeScale = .8f // Escala de volume (opcional, padrão é 1f)
                };
                SoundEvent.RequestSound(sfxArgs);

                playerInventory.AddProduct(production.outputProduct, amount);
                currentTime = 0;
                isAbleToGive = false;
                isGrown = false;
                StartCoroutine(GrowthCycle());
            }
        }
    }
}
