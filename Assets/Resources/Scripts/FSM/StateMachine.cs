using UnityEngine;
using System;
using System.Collections.Generic;
using Tcp4.Resources.Scripts.Core;

namespace Tcp4
{
     public class StateMachine
    {
        public IState CurrentState { get; private set; }
        private Dictionary<string, IState> states;
        private Dictionary<string, Func<AbilitySet, bool>> stateAbilities;
        private BaseEntity owner;

        public StateMachine(BaseEntity owner)
        {
            this.owner = owner;
            states = new Dictionary<string, IState>();
            stateAbilities = new Dictionary<string, Func<AbilitySet, bool>>();
        }

        public void Initialize(IState startingState)
        {
            CurrentState = startingState;
            CurrentState?.DoEnterLogic();
        }

        public void ChangeState(string newStateName, DynamicEntity entity)
        {
            if (states.TryGetValue(newStateName, out var newState) && CanSwitchState(newStateName, entity))
            {
                CurrentState.DoExitLogic();
                CurrentState = newState;
                CurrentState.DoEnterLogic();
            }
            else
            {
                Debug.LogWarning($"Não foi possível mudar para o estado: {newStateName}. Estado atual: {CurrentState?.GetType().Name}");
            }
        }

        public void UpdateState()
        {
            CurrentState?.DoFrameUpdateLogic();
        }

        public void PhysicsUpdateState()
        {
            CurrentState?.DoPhysicsLogic();
        }

        public void RegisterState<T>(string stateName, IInitializeState<T> state, T entity, Func<AbilitySet, bool> canSwitchFunc = null) 
            where T : BaseEntity
        {
            if (!states.ContainsKey(stateName))
            {
                state.Initialize(entity); 
                states[stateName] = state;

                if (canSwitchFunc != null)
                {
                    stateAbilities[stateName] = canSwitchFunc;
                }
            }
            else
            {
                Debug.LogWarning($"Estado já registrado: {stateName}");
            }
        }

        private bool CanSwitchState(string newStateName, DynamicEntity entity)
        {
            if (stateAbilities.TryGetValue(newStateName, out var canSwitchFunc))
            {
                var dynamicEntity = owner as DynamicEntity;
                if (dynamicEntity != null)
                {
                    return canSwitchFunc(dynamicEntity.GetAbility());
                }
            }
            return true;
        }
    }
}
