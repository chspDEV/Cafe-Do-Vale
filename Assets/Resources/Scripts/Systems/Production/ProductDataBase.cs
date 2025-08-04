using System.Collections.Generic;
using UnityEngine;

public static class ProductDatabase
{
    private static Dictionary<int, BaseProduct> rawProductById;
    private static Dictionary<int, BaseProduct> ingredientsById;
    private static bool isInitialized = false;

    public static void Initialize()
    {
        if (isInitialized) return;

        rawProductById = new Dictionary<int, BaseProduct>();
        ingredientsById = new Dictionary<int, BaseProduct>();

        BaseProduct[] rawProducts = Resources.LoadAll<BaseProduct>("Database/RawProductSO");
        BaseProduct[] ingredients = Resources.LoadAll<BaseProduct>("Database/IngredientsSO");

        foreach (var product in rawProducts)
        {
            if (product == null) continue;

            if (!rawProductById.ContainsKey(product.productID))
            {
                rawProductById.Add(product.productID, product);
            }
            else
            {
                Debug.LogWarning($"[ProductDatabase] Produto duplicado com ID: {product.productID} ({product.productName})");
            }
        }

        foreach (var product in ingredients)
        {
            if (product == null) continue;

            if (!ingredientsById.ContainsKey(product.productID))
            {
                rawProductById.Add(product.productID, product);
            }
            else
            {
                Debug.LogWarning($"[ProductDatabase] Produto duplicado com ID: {product.productID} ({product.productName})");
            }
        }

        isInitialized = true;
        Debug.Log($"[ProductDatabase] Carregado {ingredientsById.Count} produtos.");
    }

    public static BaseProduct GetRawProductByID(int id)
    {
        if (!isInitialized) Initialize();

        if (rawProductById.TryGetValue(id, out var product))
        {
            return product;
        }

        Debug.LogWarning($"[ProductDatabase] Produto com ID {id} não encontrado.");
        return null;
    }

    public static BaseProduct GetIngredientByID(int id)
    {
        if (!isInitialized) Initialize();

        if (ingredientsById.TryGetValue(id, out var product))
        {
            return product;
        }

        Debug.LogWarning($"[ProductDatabase] Produto com ID {id} não encontrado.");
        return null;
    }

    public static List<BaseProduct> GetAllProducts()
    {
        if (!isInitialized) Initialize();
        return new List<BaseProduct>(rawProductById.Values);
    }
}
