
using Tcp4;
using UnityEngine;

public class SelectProduct: MonoBehaviour
{
    public BaseProduct myProduct;

    public void Select()
    {
        CreationManager.Instance.SelectProduct(myProduct);
    }

    public void Unselect()
    {
        CreationManager.Instance.UnselectProduct(myProduct);
    }
}