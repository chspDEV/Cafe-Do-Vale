using System.Collections;
using System.Collections.Generic;
using Tcp4.Resources.Scripts.Core;
using UnityEngine;

namespace Tcp4
{
    public interface IState
    {
        void DoEnterLogic();
        void DoExitLogic();
        void DoFrameUpdateLogic();
        void DoPhysicsLogic();
        void DoChecks();
        void ResetValues();
    }
    public interface IInitializeState<T> : IState where T : BaseEntity
    {
        void Initialize(T Entity);
    }
}
