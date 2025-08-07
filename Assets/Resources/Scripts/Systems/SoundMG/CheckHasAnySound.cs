using GameResources.Project.Scripts.Utilities.Audio;
using UnityEngine;

public class CheckHasAnySound : MonoBehaviour
{
    [SerializeField] private AudioSource mySource;

    private void Start()
    {
        
        if (mySource == null)
        {
            mySource = GetComponent<AudioSource>();
            if (mySource == null)
            {
                Debug.LogWarning("Nenhum AudioSource encontrado no objeto atual.");
                return;

            }
                

        }

        SoundManager.StopMusic();

        AudioSource[] allSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        foreach (var s in allSources)
        {
            if (s != mySource && s.isPlaying)
            {
                s.Stop(); // Para outros que estão tocando
                
            }
        }

        if (!mySource.isPlaying)
        {
            mySource.Play(); // Toca se ainda não estiver tocando
        }
    }
}
