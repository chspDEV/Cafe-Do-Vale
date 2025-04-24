using ComponentUtils.ComponentUtils.Scripts;
using System.Collections;
using Tcp4.Resources.Scripts.Systems.DayNightCycle;
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

        public ShopManager shopManager;

        public CreationManager creationManager;

        public UIVirtualJoystick LeftJoystick;
        public UIVirtualJoystick RightJoystick;
        

        private void Start()
        {
            StartCoroutine(SubscribeEvents());
        }

        IEnumerator SubscribeEvents()
        {
            yield return new WaitForSeconds(1f);
            timeManager.OnOpenCoffeeShop            += uiManager.OpenShopNotification;
            timeManager.OnOpenCoffeeShop            += shopManager.AbrirPorta;
            timeManager.OnOpenCoffeeShop            += clientManager.StartSpawnClients;

            timeManager.OnCloseCoffeeShop           += uiManager.CloseShopNotification;
            timeManager.OnCloseCoffeeShop           += shopManager.FecharPorta;
            timeManager.OnCloseCoffeeShop           += clientManager.StopSpawnClients;
            timeManager.OnCloseCoffeeShop           += RestartDay;

            clientManager.OnClientSetup             += uiManager.NewClientNotification;

            storageManager.OnChangeStorage          += uiManager.UpdateStorageView;
            storageManager.OnCleanStorage           += uiManager.CleanStorageSlots;

            creationManager.OnChangeInventory       += uiManager.UpdateCreationView;
            creationManager.OnChangeInventory       += uiManager.UpdateIngredientsView;

            shopManager.OnChangeMoney               += uiManager.UpdateMoney;
            shopManager.OnChangeStar                += uiManager.UpdateStars;
            shopManager.OnChangeStar                += shopManager.CheckUpgradeStar;

            StepSound stepSound = GameAssets.Instance.player.GetComponent<StepSound>();

            LeftJoystick.OnMove                     += (input) => stepSound.SetMovementInput(input);
            RightJoystick.OnMove                    += (input) => stepSound.SetMovementInput(input);

        }

        public void RestartDay()
        {
            GameAssets.Instance.player.transform.position = GameAssets.Instance.safePoint.position;
            Instantiate(GameAssets.Instance.pfNovoDia, uiManager.hudCanvas.transform);
        }


    }
}