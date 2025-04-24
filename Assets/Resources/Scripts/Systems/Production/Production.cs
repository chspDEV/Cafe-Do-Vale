using System.Collections;
using UnityEngine;

namespace Tcp4
{

    [CreateAssetMenu(fileName = "Production", menuName = "Production/Production")]
    public class Production : ScriptableObject
    {
        public GameObject[] models;
        public BaseProduct product;
        public float timeToGrow;
        public float timeToHarvest;
    }
}