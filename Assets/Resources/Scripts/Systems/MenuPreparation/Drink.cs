using System.Collections.Generic;
using UnityEngine;

namespace Tcp4
{
    [CreateAssetMenu(fileName = "NewDrink", menuName = "Menu/Drink")]
    public class Drink : BaseProduct
    {
        public List<BaseProduct> requiredIngredients;
        public float preparationTime;
    }
}
