using UnityEngine;

namespace Tcp4
{
    [CreateAssetMenu(fileName = "New Status Effect", menuName = "Effects/New Effect")]
    public class StatusEffect : ScriptableObject
    {
        public string effectName;
        public StatusType type;
        public float duration;
        public float interval;
        public float effectValue;
        public bool isBuff;
        public bool isContinuous;
        public bool isPermanent;
        public GameObject effectPrefab;

        public StatusEffectData CreateEffectData()
        {
            return new StatusEffectData(
                effectName,
                type,
                duration,
                interval,
                effectPrefab,
                effectValue,
                isBuff,
                isContinuous,
                isPermanent
            );
        }
    }

}