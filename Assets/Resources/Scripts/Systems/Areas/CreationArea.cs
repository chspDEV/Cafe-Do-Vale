using Tcp4;
using UnityEngine;

public class CreationArea: MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            UIManager.Instance.UpdateCreationView();
            ControlMenu(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            ControlMenu(false);
        }
    }

    public void ControlMenu(bool isActive)
    {
        UIManager.Instance.ControlCreationMenu(isActive);
    }

}