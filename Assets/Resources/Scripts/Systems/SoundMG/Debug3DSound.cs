using GameResources.Project.Scripts.Utilities.Audio;
using UnityEngine;

public class Debug3DSound : MonoBehaviour
{
    public PlaySFX player;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) )
        { 
            player.Play();
        }
    }
}