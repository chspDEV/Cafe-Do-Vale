using UnityEngine;

[CreateAssetMenu(fileName = "BaseProduct", menuName = "ScriptableObjects/BaseProduct", order = 1)]
public class BaseProduct : ScriptableObject
{
    public string productName;
    public int quality;
    public byte productID;
    public GameObject model;
    public Sprite productImage;

    private static byte nextID = 0;

    private void OnEnable()
    {
        // Gera um novo ID se ainda não tiver um
        if (productID == 0)
        {
            productID = nextID++;
            if (nextID == 0) // Verifica se o contador voltou a 0 (overflow)
            {
                Debug.LogWarning("O contador de IDs voltou a 0. IDs podem não ser mais únicos.");
            }
        }
    }
}
