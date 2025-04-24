using System;
using Tcp4.Resources.Scripts.Core;
using Tcp4.Resources.Scripts.Types;

namespace Tcp4.Resources.Scripts.Interfaces
{
    public interface IInteractable
    {
        void Interact(BaseEntity interactor);
    }
}
