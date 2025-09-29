using UnityEngine;
using Tcp4.Assets.Resources.Scripts.Systems.Almanaque;
using GameEventArchitecture.Core.EventSystem.Base;

public class CuriosityBox : MonoBehaviour
{
    [Header("Curiosidade")]
    public CuriositySO curiosity;

    [Header("Evento do Almanaque")]
    public AlmanaqueGameEvent onCuriosityUnlocked;

    public bool onlyOnce = true;

    private bool hasBeenOpened = false;

    public void Interact()
    {
        if (onlyOnce && hasBeenOpened)
            return;

        OpenBox();
    }

    private void OpenBox()
    {
        hasBeenOpened = true;
        Debug.Log($"Curiosidade: {curiosity.title}");

        CuriosityAlmanaqueEntry entry = ScriptableObject.CreateInstance<CuriosityAlmanaqueEntry>();
        entry.title = curiosity.title;
        entry.description = curiosity.description;

        if (onCuriosityUnlocked != null)
            onCuriosityUnlocked.Broadcast(entry);

        // abrir ui
    }
}
