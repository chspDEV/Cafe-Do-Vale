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
        [TabGroup("Refinamento")] public Sprite sprError;
        [TabGroup("Refinamento")] public Sprite sprProductionWait;
        [TabGroup("Refinamento")] public Sprite sprRefinamentWait;
        [TabGroup("Refinamento")] public Sprite sprSpoilingWarning;

        [TabGroup("Gerais")] [HideInInspector] public Sprite sprInteraction;
        [TabGroup("Gerais")] public Sprite ready;
        [TabGroup("Gerais")] public Sprite transparent;

        [TabGroup("Interacao")]
        [SerializeField] private Sprite inputXBOX;
        [TabGroup("Interacao")]
        [SerializeField] private Sprite inputPLAYSTATION;
        [TabGroup("Interacao")]
        [SerializeField] private Sprite inputPC;

        public List<Sprite> clientSprites;
        public List<string> clientNames;

        public List<GameObject> clientModels;
        public GameObject pfNovoDia;
        public Sprite Money;
        public GameObject player;
        public Transform safePoint;
        public PlayerMovement playerMovement;

        public void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");

            if(player != null)
                playerMovement = player.GetComponent<PlayerMovement>();

            sprInteraction = inputPLAYSTATION == null ? inputPC : inputPLAYSTATION;
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