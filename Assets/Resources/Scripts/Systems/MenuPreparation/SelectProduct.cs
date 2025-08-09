using Tcp4;
using UnityEngine;

public class SelectProduct : MonoBehaviour
{
    public BaseProduct myProduct;

    public void Select()
    {
        if (!CreationManager.Instance.CanAdd()) return;

        if (myProduct != null)
            CreationManager.Instance.SelectProduct(myProduct);
    }

    public void Unselect()
    {
        if (myProduct != null)
            CreationManager.Instance.UnselectProduct(myProduct);
    }


}
