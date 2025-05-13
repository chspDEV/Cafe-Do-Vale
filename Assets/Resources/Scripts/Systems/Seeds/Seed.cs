using Tcp4;
using UnityEngine;

[CreateAssetMenu(fileName = "Nova semente", menuName = "Production/Seed")]
public class Seed : ScriptableObject
{
    public Production targetProduction;
    public int purchaseCost;
    public Sprite seedIcon;
}