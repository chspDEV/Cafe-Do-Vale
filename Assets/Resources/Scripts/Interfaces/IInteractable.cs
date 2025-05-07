using UnityEngine;
using UnityEngine.UI;


public interface IInteractable
{
    void OnFocus();
    void OnInteract();
    void OnLostFocus();
    bool IsInteractable();
}

