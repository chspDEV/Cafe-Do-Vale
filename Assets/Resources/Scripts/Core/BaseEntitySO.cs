using UnityEngine;

namespace Tcp4.Resources.Scripts.Core
{
    [CreateAssetMenu(fileName = "BaseEntity", menuName = "Entity/Create new Entity")]
    public class BaseEntitySO : ScriptableObject
    {
        [Header("Entity Info")]
        [Tooltip("Name of the entity")]
        public string Name;
        
        [Tooltip("Unique identifier for the entity")]
        public byte Id;

        [Header("Entity Status")]
        [Tooltip("List of base stats for the entity")]
        [SerializeField] private BaseStatus[] baseStats;

        [Header("Base Ability States")]
        public AbilitySet abilitySet;

        private void OnEnable()
        {
            if (baseStats == null || baseStats.Length == 0)
            {
                InitializeStatus();
            }
        }

        private void InitializeStatus()
        {
            int statusCount = System.Enum.GetValues(typeof(StatusType)).Length;
            baseStats = new BaseStatus[statusCount];
            for (int i = 0; i < statusCount; i++)
            {
                StatusType statusType = (StatusType)i;
                baseStats[i] = new BaseStatus { statusType = statusType, value = 0, statusName = statusType.ToString() };
            }
        }

        public float GetStatusValue(StatusType statusType)
        {
            foreach (BaseStatus status in baseStats)
            {
                if (status.statusType == statusType)
                    return status.value;
            }
            return 0f;
        }

        public AbilitySet GetBaseAbilitySet()
        {
            return abilitySet;
        }

        public BaseStatus[] GetBaseStatus()
        {
            return baseStats;
        }

        public void ModifyAbility(AbilitySet newAbilities)
        {
            abilitySet = newAbilities;
        }
    }
}
