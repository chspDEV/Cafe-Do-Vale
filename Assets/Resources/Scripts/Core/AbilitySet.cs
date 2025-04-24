using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tcp4
{
    [Serializable]
    public class AbilitySet
    {
        [SerializeField] private List<Ability> abilities;

        public AbilitySet()
        {
            abilities = new List<Ability>();
        }

        public void AddAbility(AbilityType abilityType, bool defaultValue)
        {
            abilities.Add(new Ability { abilityType = abilityType, isActive = defaultValue });
        }

        public bool GetAbilityValue(AbilityType abilityType)
        {
            foreach (var ability in abilities)
            {
                if (ability.abilityType == abilityType)
                    return ability.isActive;
            }
            return false;
        }

        public void SetAbilityValue(AbilityType abilityType, bool value)
        {
            for (int i = 0; i < abilities.Count; i++)
            {
                if (abilities[i].abilityType == abilityType)
                {
                    abilities[i] = new Ability { abilityType = abilityType, isActive = value };
                    return;
                }
            }
            abilities.Add(new Ability { abilityType = abilityType, isActive = value });
        }

    }

    [System.Serializable]
    public struct Ability
    {
        public AbilityType abilityType;
        public bool isActive;
    }

    public enum AbilityType
    {
        CanMove,
        CanAirborne,
        CanInteract,
        CanFly,
        CanSwim,
        CanDash,
        CanAttack
    }

}
