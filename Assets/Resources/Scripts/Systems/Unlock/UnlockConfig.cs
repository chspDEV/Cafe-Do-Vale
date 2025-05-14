using System.Collections.Generic;
using Tcp4;
using UnityEngine;

[CreateAssetMenu(menuName = "Unlock System/Unlock Config")]
public class UnlockConfig : ScriptableObject
{
    public List<UnlockableItem<Production>> unlockableProductions = new();
    public List<UnlockableItem<Drink>> unlockableDrinks = new();
    public List<UnlockableItem<GameObject>> unlockableCups = new();
}
