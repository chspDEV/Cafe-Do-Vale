using System;
using Tcp4.Resources.Scripts.Core;
using Tcp4.Resources.Scripts.Interfaces;

namespace Tcp4.Resources.Scripts.Systems.Interaction
{
    public static class InteractionEvents
    {
        public static event Action<IInteractable> OnInteractionAvailable;
        public static event Action OnInteractionUnavailable;
        public static event Action<IInteractable, BaseEntity> OnInteractionStarted;
        public static event Action OnInteractionEnded;

        public static void TriggerInteractionAvailable(IInteractable interactable) => 
            OnInteractionAvailable?.Invoke(interactable);
    
        public static void TriggerInteractionUnavailable() => 
            OnInteractionUnavailable?.Invoke();
    
        public static void TriggerInteractionStarted(IInteractable interactable, BaseEntity interactor) => 
            OnInteractionStarted?.Invoke(interactable, interactor);
    
        public static void TriggerInteractionEnded() => 
            OnInteractionEnded?.Invoke();
    }
}