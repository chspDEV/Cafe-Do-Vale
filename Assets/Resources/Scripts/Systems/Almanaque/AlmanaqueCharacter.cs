using GameEventArchitecture.Core.EventSystem.Listeners;
using Tcp4.Assets.Resources.Scripts.Systems.Almanaque;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAlmanaqueCharacter", menuName = "Almanaque/Character")]
public class AlmanaqueCharacter : BaseAlmanaque
{
    public string characterName;
    public string description;
    public string phrase;
    //public string age;
    public Sprite characterImage;
    public bool isUnlocked = false;
    public int index;

    public VoidGameEvent OnOpenMoreInfo;

    private void OnEnable()
    {
#if UNITY_EDITOR
        isUnlocked = false;
#endif
    }
}