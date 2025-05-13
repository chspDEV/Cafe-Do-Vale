using ComponentUtils.ComponentUtils.Scripts;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tcp4.Assets.Resources.Scripts.Managers
{
    public class GameAssets : Singleton<GameAssets>
    {
        [TabGroup("Sprites")] public Sprite sprError;
        [TabGroup("Sprites")] public Sprite sprProductionWait;
        [TabGroup("Sprites")] public Sprite sprRefinamentWait;
        [TabGroup("Sprites")] public Sprite sprSpoilingWarning;
        [TabGroup("Sprites")] public Sprite ready;
        [TabGroup("Sprites")] public Sprite transparent;

        public List<Sprite> clientSprites;
        public List<string> clientNames;

        public List<GameObject> clientModels;
        public GameObject pfNovoDia;
        public Sprite Money;
        public GameObject player;
        public Transform safePoint;
        public void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        public static string GenerateID(int tamanho)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new System.Random();
            var result = new string(
                Enumerable.Repeat(chars, tamanho)
                        .Select(s => s[random.Next(s.Length)])
                        .ToArray());
            return result;
        }
    }
}