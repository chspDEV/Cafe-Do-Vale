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
        private TimeManager timeManager;

        public event System.Action<ProductionArea, BaseProduct> OnProductionComplete;


        // NOVA VARIÁVEL DE CONTROLE
        private bool isTaskedForHarvest = false;

        private void CreateWorkerTask()
        {
            // Só cria a tarefa se:
            // - Não há tarefa ativa para esta colheita
            // - Há produto pronto para colher
            // - WorkerManager existe
            if (isTaskedForHarvest || !HasHarvestableProduct()) return;

            if (WorkerManager.Instance != null && production != null && production.outputProduct != null)
            {
                isGrown = true;
                isAbleToGive = true;
                isTaskedForHarvest = true; // Reserva ANTES de criar a tarefa
                Debug.Log($"[ProductionArea] Área {areaID}: Criando tarefa de colheita para {production.outputProduct.productName}");
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

            // O registro do ID deve ser feito aqui ou em um método de inicialização centralizado
            if (WorkerManager.Instance != null && GameAssets.Instance != null)
            {
                areaID = GameAssets.Instance.GenerateAreaID();
                WorkerManager.Instance.RegisterProductionStation(areaID, this);
            }
        }

        private void OnDisable()
        {
            ProductionManager.Instance.OnChooseProduction -= SelectProduction;
            if (timeManager != null)
            {
                timeManager.OnTimeMultiplierChanged -= ReSetupMaxTime;
            }

            if (minigameTrigger != null && minigameTrigger.minigameToStart != null)
                minigameTrigger.minigameToStart.OnGetReward -= this.ResetGrowthCycle;
        }

        public override void OnInteract()
        {
            // BLOQUEIA A INTERAÇÃO DO JOGADOR SE UM WORKER ESTIVER A CAMINHO
            /*if (isTaskedForHarvest)
            {
                Debug.Log($"[ProductionArea] Interação bloqueada. A colheita na área {areaID} está reservada para um trabalhador.");
                SoundEventArgs sfxArgs = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "erro",
                    Position = transform.position,
                    VolumeScale = 0.4f
                };
                SoundEvent.RequestSound(sfxArgs);
                return;
            }
            */

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
            // Impede o foco se a colheita estiver reservada
            if (isTaskedForHarvest)
            {
                base.OnLostFocus(); // Força a perda de foco
                return;
            }
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
            if (timeImage != null && timeManager != null)
                timeImage.SetupMaxTime(production.timeToGrow);
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

            // NOVO: Adiciona um feedback visual se a tarefa está reservada
            if (isTaskedForHarvest && isAbleToGive)
            {
                timeImage.ChangeSprite(GameAssets.Instance.sprProductionWait); // Use um ícone de "reservado" aqui
            }
            else if (isAbleToGive)
            {
                timeImage.ChangeSprite(GameAssets.Instance.ready);
            }
            else if (currentTime < production.timeToGrow * timeManager.timeMultiplier)
            {
                timeImage.ChangeSprite(GameAssets.Instance.sprProductionWait);
            }
        }

        IEnumerator OpenProductionCoroutine()
        {
            yield return new WaitForSeconds(timeToOpenInterface);
            if (playerInventory != null)
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

        // Correções para ProductionArea.cs

        // 1. CORRIGIR o método GrowthCycle - Não resete a reserva no final
        private IEnumerator GrowthCycle()
        {
            if (production == null || timeManager == null)
            {
                yield break;
            }

            OnLostFocus();
            var models = production.models;
            float startTime = timeManager.CurrentHour;
            float targetTime = startTime + production.timeToGrow;
            bool hasModels = models != null && models.Length > 0;

            while (timeManager.CurrentHour < targetTime)
            {
                float elapsedTime = timeManager.CurrentHour - startTime;
                currentTime = Mathf.Clamp(elapsedTime, 0, production.timeToGrow);
                UpdateCurrentTime();

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
                            currentModel.transform.SetPositionAndRotation(pointToSpawn.position, models[modelIndex].transform.rotation);
                        }
                    }
                }
                yield return null;
            }

            EnableInteraction();
            isGrown = true;
            isAbleToGive = true;
            // REMOVIDO: isTaskedForHarvest = false; - Não resetar aqui!

            // Dispara evento para integração - e já cria a tarefa automaticamente
            if (production != null && production.outputProduct != null)
            {
                Debug.Log($"[ProductionArea] Área {areaID}: produção concluída ({production.outputProduct.name}). Criando tarefa para worker.");
                OnProductionComplete?.Invoke(this, production.outputProduct);

                // Criar tarefa automaticamente quando a produção termina
                CreateWorkerTask();
            }
        }

        private void HarvestProduct()
        {
            if (isAbleToGive && isGrown && playerInventory != null && playerInventory.CanStorage())
            {
                if (minigameTrigger == null || minigameTrigger.minigameToStart == null) return;
                if (production == null || production.outputProduct == null) return;

                minigameTrigger.minigameToStart.OnGetReward += this.ResetGrowthCycle;
                minigameTrigger.minigameToStart.SetupReward(production.outputProduct);
                GameAssets.Instance.SetupLastIconMinigamePlants(production.outputProduct.productImage);
                minigameTrigger.TriggerMinigame();
                DisableInteraction();
                InteractionManager.Instance.UpdateLastId(production.outputProduct.productName);
            }
            else
            {
                SoundEventArgs sfxArgs = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "erro",
                    VolumeScale = 0.5f
                };
                SoundEvent.RequestSound(sfxArgs);

                NotificationManager.Instance.Show("Inventario Cheio!", "Sem espaços livres.", production.outputProduct.productImage);
            }
        }

        public bool HarvestProductFromWorker()
        {
            // VERIFICAÇÃO CRÍTICA: Proteger contra chamadas múltiplas
            if (!HasHarvestableProduct())
            {
                Debug.LogWarning($"[ProductionArea] ⚠ Tentativa de colheita por worker falhou. Área {areaID} não tem produto pronto ou já foi colhida.");
                ReleaseReservation(); // Liberar reserva mesmo se falhar
                return false;
            }

            // VERIFICAÇÃO ADICIONAL: Se não está reservada, algo está errado
            if (!isTaskedForHarvest)
            {
                Debug.LogWarning($"[ProductionArea] ⚠ Área {areaID} não estava reservada para worker! Colheita não autorizada.");
                return false;
            }

            string productName = production.outputProduct != null ? production.outputProduct.productName : "Produto Desconhecido";
            Debug.Log($"[ProductionArea] ✓ Worker colheu: {productName} da área {areaID}");

            // Reset IMEDIATO do estado para evitar dupla colheita
            isAbleToGive = false;
            isGrown = false;
            ReleaseReservation(); // Liberar reserva imediatamente
            currentTime = 0;

            // Limpar modelo visual atual
            if (currentModel != null && objectPools != null)
            {
                objectPools.Return(currentModel);
                currentModel = null;
            }

            // Iniciar novo ciclo de crescimento
            StartCoroutine(GrowthCycle());
            return true;
        }

        public void ReserveForWorker()
        {
            if (!HasHarvestableProduct())
            {
                Debug.LogWarning($"[ProductionArea] Tentativa de reservar área {areaID} sem produto pronto!");
                return;
            }

            isTaskedForHarvest = true;
            Debug.Log($"[ProductionArea] Área {areaID} reservada para worker");
        }

        public void ReleaseReservation()
        {
            isTaskedForHarvest = false;
            Debug.Log($"[ProductionArea] Área {areaID} liberada da reserva");
        }

        public bool IsReservedForWorker()
        {
            return isTaskedForHarvest;
        }   


        public bool HasHarvestableProduct()
        {
            return isAbleToGive && isGrown;
        }

        public BaseProduct GetCurrentProduct()
        {
            if (HasHarvestableProduct())
                return production.outputProduct;
            return null;
        }

        void ResetGrowthCycle()
        {
            if (minigameTrigger != null && minigameTrigger.minigameToStart != null)
                minigameTrigger.minigameToStart.OnGetReward -= this.ResetGrowthCycle;

            // Se o jogador colheu, cancelar qualquer tarefa de worker pendente
            if (isTaskedForHarvest)
            {
                Debug.Log($"[ProductionArea] Jogador colheu área {areaID} - cancelando tarefa de worker");
                ReleaseReservation();

                // Notificar o WorkerManager para cancelar a tarefa (se houver uma forma de fazer isso)
                // Alternativa: o worker vai falhar naturalmente quando tentar coletar
            }

            currentTime = 0;
            isAbleToGive = false;
            isGrown = false;

            // Limpar modelo visual
            if (currentModel != null && objectPools != null)
            {
                objectPools.Return(currentModel);
                currentModel = null;
            }

            StartCoroutine(GrowthCycle());
        }
    }
}