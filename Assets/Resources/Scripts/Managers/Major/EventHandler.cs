using System.Collections;
using UnityEngine;
using Tcp4.Assets.Resources.Scripts;

namespace Tcp4.Assets.Resources.Scripts.Managers
{
    public class EventHandler : Singleton<EventHandler>
    {
    
        public ClientManager clientManager;
        public UIManager uiManager;
        public TimeManager timeManager;
        public StorageManager storageManager;
        public SeedManager seedManager;

        public ShopManager shopManager;
        public UnlockManager unlockManager;

        public CreationManager creationManager;

        //public UIVirtualJoystick LeftJoystick;
        //public UIVirtualJoystick RightJoystick;
        

        private void Start()
        {
            StartCoroutine(SubscribeEvents());
        }

        IEnumerator SubscribeEvents()
        {
            yield return new WaitForSeconds(1f);
            timeManager.OnOpenCoffeeShop            += uiManager.OpenShopNotification;
            timeManager.OnOpenCoffeeShop            += clientManager.StartSpawnClients;
            timeManager.OnOpenCoffeeShop            += clientManager.OpenShop;

            timeManager.OnCloseCoffeeShop           += uiManager.CloseShopNotification;
            timeManager.OnCloseCoffeeShop           += clientManager.StopSpawnClients;
            timeManager.OnCloseCoffeeShop           += clientManager.CloseShop;


            clientManager.OnClientSetup             += uiManager.NewClientNotification;


            storageManager.OnChangeStorage          += uiManager.UpdateStorageView;
            storageManager.OnCleanStorage           += uiManager.CleanStorageSlots;

            creationManager.OnChangeInventory       += uiManager.UpdateCreationView;
            creationManager.OnChangeInventory       += uiManager.UpdateIngredientsView;

            shopManager.OnChangeMoney               += uiManager.UpdateMoney;
            shopManager.OnChangeStar                += uiManager.UpdateStars;

            unlockManager.OnProductionsUpdated      += seedManager.seedShop.PopulateShop;
            timeManager.OnResetDay                  += seedManager.seedShop.PopulateShop;

            //StepSound stepSound = GameAssets.Instance.player.GetComponent<StepSound>();

            //LeftJoystick.OnMove                     += (input) => stepSound.SetMovementInput(input);
            //RightJoystick.OnMove                    += (input) => stepSound.SetMovementInput(input);

            timeManager.OnTimeChanged += HandleTimeChange;
            //timeManager.OnDayNightChanged += HandleDayNightChange;
        }

        void HandleTimeChange(float hour)
        {
            // Atualizar UI de hora
            uiManager.UpdateClock(hour);
        }

        void HandleDayNightChange(bool isDay)
        {
            // Executar transições de dia/noite
            //if(isDay) StartCoroutine(SunriseTransition());
            //else StartCoroutine(SunsetTransition());
        }

        public void RestartDay()
        {
            //GameAssets.Instance.player.transform.position = GameAssets.Instance.safePoint.position;
            //Instantiate(GameAssets.Instance.pfNovoDia, uiManager.hudCanvas.transform);
        }
    }
}