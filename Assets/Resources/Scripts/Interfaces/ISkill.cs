using System.Collections;
using System.Collections.Generic;
using Tcp4.Resources.Scripts.Core;
using UnityEngine;

namespace Tcp4
{
    public interface ISkill
    {
        void ExecuteSkill(DynamicEntity player);
        int GetCooldown();
    }
}
