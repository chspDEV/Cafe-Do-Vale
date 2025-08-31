using UnityEngine;
using GameEventArchitecture.Core.EventSystem.Listeners;
using NUnit.Framework;
using System.Collections.Generic;
using Sirenix.Utilities;
using Tcp4.Assets.Resources.Scripts.Managers;

public class AlmanaqueManager : MonoBehaviour
{
    public AlmanaqueGameEvent OnUnlockCharacter;

    public List<AlmanaqueCharacter> allCharacters;

    private void Start()
    {
        var loadedCharacters = Resources.LoadAll<AlmanaqueCharacter>("Database/AlmanaqueSO/Characters");
        allCharacters = new List<AlmanaqueCharacter>(loadedCharacters);

        allCharacters.Sort((x, y) => x.index.CompareTo(y.index));
    }

    private void Update()
    {
        if (GameAssets.Instance.isDebugMode)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                foreach (var character in allCharacters)
                {
                    if (character != null && !character.isUnlocked)
                    {
                        UnlockCharacter(character.index);
                    }
                }
            }
        }
    }

    public void UnlockCharacter(int index)
    {
        if (allCharacters[index] != null)
        {
            OnUnlockCharacter.Broadcast(allCharacters[index]);
            NotificationManager.Instance.Show("Almanaque Atualizado!", "Personagem desbloqueado: " + allCharacters[index].characterName, allCharacters[index].characterImage);
        }
        else
        {
            Debug.LogWarning("[AlmanaqueManager] Character at index " + index + " not found.");
        }
    }
}