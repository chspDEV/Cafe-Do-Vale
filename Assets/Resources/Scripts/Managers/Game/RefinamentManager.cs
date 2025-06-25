using System.Collections.Generic;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;

namespace Tcp4
{
    public class RefinamentManager : Singleton<RefinamentManager>
    {

        [Header("Config")]
        [SerializeField] private List<RefinementRecipe> recipes;
        [SerializeField] private List<Drink> drinks;

        [Header("Debug")]
        [SerializeField] private List<Ingredients> debugIngredients;

        public BaseProduct Refine(BaseProduct inputProduct)
        {
            foreach (var r in recipes)
            {
                if (r.inputProduct == inputProduct)
                {
                    return r.outputProduct;
                }
            }

            Debug.LogError("Nenhuma receita de refinamento encontrada para o produto: " + inputProduct.productName);
            return null;
        }

        public Drink CreateDrink(List<BaseProduct> inputIngredients)
        {
            foreach(var d in drinks)
            {
                int max = d.requiredIngredients.Count;
                int c = 0;

                //Validando ingredientes
                for(var i = 0; i < max; i++)
                {
                    if(d.requiredIngredients[i] == inputIngredients[i])
                    {
                        c++;
                    }
                }

                //Conferindo se receita bate e passando qualidade
                if(c == max)
                {
                    int newQ = 0;

                    foreach(var i in inputIngredients) {newQ += i.quality;}
                    d.quality = newQ;
                    Debug.Log($"Receita feita: {d} com {d.quality} de qualidade!");
                    return d;
                }
            }

            Debug.LogError("Nenhuma receita de drink encontrada para os ingredientes!");
            return null;
        }

        public List<RefinementRecipe> GetRecipes() => recipes;
        public Drink GetDrinkByListID(int ID)
        {
            for(var i = 0; i < drinks.Count; i++)
            {
                if(i == ID)
                {
                    return drinks[i];
                }
            }

            return null;
        }

        public Drink GetDrinkByID(int ID)
        {
            foreach (var d in drinks)
            {
                if (d.productID == ID)
                {
                    return d;
                }
            }

            return null;
        }

        private void Update()
        {
            if (GameAssets.Instance.isDebugMode)
            {
                if (Input.GetKeyDown(KeyCode.H))
                {
                    Inventory i = GameAssets.Instance.player.GetComponent<Inventory>();
                    i.UpdateLimit(90);

                    foreach (var ingredient in debugIngredients)
                        i.AddProduct(ingredient, 3);
                }
            }
        }
    }

    [System.Serializable]
    public class RefinementRecipe
    {
        public BaseProduct inputProduct;
        public BaseProduct outputProduct;
    }
}
