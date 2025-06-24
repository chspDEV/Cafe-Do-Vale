using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameResources.Project.Scripts.Utilities.Audio;

namespace Tcp4.Assets.Resources.Scripts.Managers
{
    public class ShopManager : Singleton<ShopManager>
    {
        // Sistema de Moeda
        [SerializeField] private int money = 0;
        public event Action OnChangeMoney;

        // Sistema de Reputação
        [SerializeField] private float stars = 0f;
        public event Action OnChangeStar;
        private readonly float MaxStar = 1000f;

        public int cupLevel = 1;

        // Referências
        [SerializeField] private List<GameObject> cupPrefabs = new();
        public Transform point;
        public Transform cupHolder;


        #region Sistema de Moeda
        public void AddMoney(int value)
        {
            money += value;
            OnChangeMoney?.Invoke();

            //Fazendo o request de sfx
            SoundEventArgs sfxArgs = new()
            {
                Category = SoundEventArgs.SoundCategory.SFX,
                AudioID = "dinheiro", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                VolumeScale = .7f // Escala de volume (opcional, padrão é 1f)
            };
            SoundEvent.RequestSound(sfxArgs);
        }

        public bool TrySpendMoney(int value)
        {
            if (money >= value)
            {
                money -= value;
                OnChangeMoney?.Invoke();

                //Fazendo o request de sfx
                SoundEventArgs sfxArgs = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "dinheiro", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                    VolumeScale = .7f // Escala de volume (opcional, padrão é 1f)
                };
                SoundEvent.RequestSound(sfxArgs);

                return true;
            }
            Debug.Log("Dinheiro insuficiente!");

            //Fazendo o request de sfx
            SoundEventArgs sfxArgs1 = new()
            {
                Category = SoundEventArgs.SoundCategory.SFX,
                AudioID = "erro", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                VolumeScale = .7f // Escala de volume (opcional, padrão é 1f)
            };
            SoundEvent.RequestSound(sfxArgs1);
            return false;
        }

        public int GetMoney() => money;
        #endregion

        #region Sistema de Reputação
        public void AddStars(float value)
        {
            stars = Mathf.Clamp(stars + value, 0, MaxStar);
            OnChangeStar?.Invoke();
        }

        public float GetStars() => stars;
        public float GetMaxStars() => MaxStar;
        #endregion

        #region Sistema de Copos
        public void SpawnCup(Drink d)
        {
            if (!UnlockManager.Instance.CurrentMenu.Contains(d))
            {
                Debug.LogWarning("Tentativa de preparar drink não desbloqueado!");
                return;
            }

            int currentCupLevel = UnlockManager.Instance.GetCurrentReputationLevel();
            GameObject go = Instantiate(cupPrefabs[currentCupLevel], cupHolder);
            Cup cup = go.GetComponent<Cup>();
            cup.myDrink = d;
            cup.point = this.point;
            //Fazendo o request de sfx
            SoundEventArgs sfxArgs = new()
            {
                Category = SoundEventArgs.SoundCategory.SFX,
                AudioID = "concluido", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                Position = transform.position, // Posição para o som 3D
                VolumeScale = .8f // Escala de volume (opcional, padrão é 1f)
            };
            SoundEvent.RequestSound(sfxArgs);
        }
        #endregion

        #region Initialization
        void Start()
        {
            StartCoroutine(InitialSetup());
        }

        IEnumerator InitialSetup()
        {
            yield return new WaitForSeconds(.3f);
            OnChangeMoney?.Invoke();
            OnChangeStar?.Invoke();
        }
        #endregion

        #region Cheats (Opcional)
        void Update()
        {

            if (Input.GetKeyDown(KeyCode.H))
            {
                AddMoney(1000);
                AddStars(200);
            }

        }

        internal bool HasMoney(float remaining)
        {
            return money >= remaining;
        }
        #endregion
    }
}