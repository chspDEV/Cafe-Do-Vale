using System.Collections;
using UnityEngine;

namespace Tcp4
{

    [CreateAssetMenu(fileName = "Production", menuName = "Production/Production")]
    public class Production : ScriptableObject
    {
        public GameObject[] models;
        public BaseProduct outputProduct;
        public float timeToGrow;
        public float timeToRegrow;
        public Sprite readyIcon;
        //public float timeToHarvest;
    }
}