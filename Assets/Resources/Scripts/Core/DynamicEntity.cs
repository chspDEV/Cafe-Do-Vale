using System;
using Tcp4.Assets.Resources.Scripts.Core;

namespace Tcp4.Resources.Scripts.Core
{
    public abstract class DynamicEntity : BaseEntity
    {
        public StateMachine Machine;
        public event Action<AbilityType> OnAbilityUnlocked;
        public Movement Movement;
        
        public override void Awake()
        {
            base.Awake();
            Machine = new StateMachine(this);
        }
        
        protected virtual void Update() => Machine?.UpdateState();
        
        protected virtual void FixedUpdate() => Machine?.PhysicsUpdateState();

        public AbilitySet GetAbility() => baseStatus.GetBaseAbilitySet();

        protected void UnlockAbility(AbilityType ability)
        {
            baseStatus.abilitySet.SetAbilityValue(ability, true);
            OnAbilityUnlocked?.Invoke(ability);
        }
    }
}
