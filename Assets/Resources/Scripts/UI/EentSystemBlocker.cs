using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class EventSystemBlocker : Singleton<EventSystemBlocker>
{
    /// <summary>
    /// Chame este método para bloquear input de UI por alguns segundos.
    /// </summary>
    public static void BlockForSeconds(float seconds)
    {
        if (Instance == null)
        {
            var go = new GameObject("EventSystemBlocker");
            go.AddComponent<EventSystemBlocker>();
        }

        Instance.StartCoroutine(Instance.BlockInputCoroutine(seconds));
    }

    private IEnumerator BlockInputCoroutine(float seconds)
    {
        var eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogWarning("EventSystemBlocker: Nenhum EventSystem encontrado.");
            yield break;
        }

#if ENABLE_INPUT_SYSTEM
        var inputModuleNew = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModuleNew != null)
        {
            inputModuleNew.enabled = false;
            yield return new WaitForSecondsRealtime(seconds);
            inputModuleNew.enabled = true;
            yield break;
        }
#endif

        var inputModuleOld = eventSystem.GetComponent<StandaloneInputModule>();
        if (inputModuleOld != null)
        {
            inputModuleOld.enabled = false;
            yield return new WaitForSecondsRealtime(seconds);
            inputModuleOld.enabled = true;
        }
    }
}
