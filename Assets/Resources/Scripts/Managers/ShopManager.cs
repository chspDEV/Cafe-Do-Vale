using System;
using System.Collections;
using System.Collections.Generic;
using ComponentUtils.ComponentUtils.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tcp4.Assets.Resources.Scripts.Managers
{
    public class ShopManager : Singleton<ShopManager>
    {
        [SerializeField] private float stars = 0f;

        [SerializeField] private int cupLevel = 0;
        [SerializeField] private List<GameObject> cupPrefabs = new();

        //Caminhos para o copo
        public Transform point;
        public Transform cupHolder;
        private readonly float MaxStar = 1000f;

        RefinamentManager refManager;
        ProductionManager prodManager;
        UIManager uiManager;

        [SerializeField] private int money = 0;

        public event Action OnChangeMoney, OnChangeStar;

        public GameObject porta;
        
        [SerializeField] private List<Drink> menu;

        public void IncreaseCupLevel() 
        {
            cupLevel ++; 
            cupLevel = Mathf.Clamp(cupLevel, 0, 2);
        }

        public void IncreaseMoney(int value) 
        {
             money += value;
             OnChangeMoney.Invoke();
        }
        public void DecreaseMoney(int value) 
        { 
            if(money >= value)
            {
                money -= value; 
                OnChangeMoney.Invoke();
            }
            else
            {
                Debug.Log("Voce está pobre demais para diminuir o dinheiro...");
            }
            
        }
        public void IncreaseStar(float value) 
        {
            stars += value; 
            OnChangeStar.Invoke();
            if(stars > MaxStar) {stars = MaxStar;}
        }

        public void DecreaseStar(float value) 
        {
            stars -= value;
            OnChangeStar.Invoke();
            if(stars < 0) {stars = 0;}
        }

        public void AddNewDrink(Drink drink) 
        {
            menu.Add(drink);
        }

        public void AbrirPorta()
        {
            porta.SetActive(false);
        }

        public void FecharPorta()
        {
            porta.SetActive(true);
        }

        //Getters
        public float GetStars() => stars;
        public float GetMaxStars() => MaxStar;
        public int GetMoney() => money;

        public List<Drink> GetMenu() => menu;


        public void CheckUpgradeStar()
        {
            if (refManager == null || prodManager == null)
            {
                Debug.LogError("Gerenciadores Nulos!!!");
                return;
            }

            // Converte a pontuação para um nível de 0 a 5
            int starLevel = Mathf.FloorToInt(stars / MaxStar * 5f);

            if(starLevel < 0) starLevel = 0;

            Debug.Log("Nivel de Estrela: " + starLevel.ToString());

            // Lista de IDs das bebidas desbloqueáveis por nível
            int[][] drinkLevels = new int[][]
            {
                new int[] { 0 },                   // Nível 0: Expresso
                new int[] { 0, 1 },                // Nível 1: + Latte
                new int[] { 0, 1, 2 },             // Nível 2: + Chocolate Quente
                new int[] { 0, 1, 2, 3, 4 },       // Nível 3: + Cappuccino e Termogênico
                new int[] { 0, 1, 2, 3, 4, 5 }     // Nível 4: + Melado
            };

            // Lista de IDs das produções desbloqueáveis por nível
            int[][] productionLevels = new int[][]
            {
                new int[] { 0 },  // Nível 0 café
                new int[] { 0, 1 }, // Nível 1 + leite
                new int[] { 0, 1, 2 }, // Nível 2 + cacau
                new int[] { 0, 1, 2, 3 }, // Nível 3 + gengibre
                new int[] { 0, 1, 2, 3, 4 } // Nível 4 + mel
            };

            // Atualiza o menu de bebidas
            if (starLevel < drinkLevels.Length)
            {
                menu.Clear();
                foreach (int drinkID in drinkLevels[starLevel])
                {
                    Debug.Log($"Atualizando Drink Level {starLevel}, Drinks: {drinkID}");
                    menu.Add(refManager.GetDrinkByID(drinkID));
                }
            }

            // Atualiza a lista de produções
            List<Production> possibleProductions = new();

            if (starLevel < productionLevels.Length)
            {
                uiManager.ClearProductionCards();

                foreach (int productionID in productionLevels[starLevel])
                {
                    Debug.Log($"Atualizando Producoes, Level: {starLevel}, Producoes: {productionID}");
                    possibleProductions.Add(prodManager.GetProductionByID(productionID));
                }

                foreach(Production p in possibleProductions)
                {
                    uiManager.CreateNewProductionCard(p);
                }
            }

        }

        public void SpawnCup(Drink d)
        {
            GameObject go = Instantiate(cupPrefabs[cupLevel], cupHolder);
            Cup cup =  go.GetComponent<Cup>();
            cup.myDrink = d;
            cup.point = this.point;
            SoundManager.PlaySound(SoundType.concluido, 0.6f);
        }

        void Update()
        {
#if UNITY_EDITOR
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                IncreaseMoney(1000);
                IncreaseStar(200);
            }
#endif
        }

        void Start()
        {
            refManager  = RefinamentManager.Instance;
            prodManager = ProductionManager.Instance;
            uiManager = UIManager.Instance;

            StartCoroutine(InitialSetup());
        }

        IEnumerator InitialSetup()
        {
            yield return new WaitForSeconds(.3f);
            OnChangeMoney?.Invoke();
            OnChangeStar?.Invoke();
            CheckUpgradeStar();
        }

    }

    
}