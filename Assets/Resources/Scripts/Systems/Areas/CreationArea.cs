using Tcp4;
using UnityEngine;

public class CreationArea : BaseInteractable
{
    public override void OnInteract()
    {
        base.OnInteract();
        UIManager.Instance.UpdateCreationView();
        ControlMenu(true);
    }

    public override void OnLostFocus()
    {
        base.OnLostFocus();
        ControlMenu(false);
    }

    public void ControlMenu(bool isActive)
    {
        UIManager.Instance.ControlCreationMenu(isActive);
    }
}
