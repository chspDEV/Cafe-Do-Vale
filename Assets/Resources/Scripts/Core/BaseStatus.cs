using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tcp4
{
    [System.Serializable]
    public struct BaseStatus
    {
        [HideInInspector] public string statusName;
        public StatusType statusType;
        public float value;
    }
}
