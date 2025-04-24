using Tcp4.Resources.Scripts.Core;
using Tcp4.Resources.Scripts.Interfaces;
using Tcp4.Resources.Scripts.Systems.CollisionCasters;
using UnityEngine;

namespace Tcp4.Resources.Scripts.Systems.Interaction
{
    public class InteractableHandler : MonoBehaviour
    {
        [SerializeField]private BaseEntity currentTarget;
        [SerializeField]private IInteractable currentInteractable;
        [SerializeField]private BaseEntity ownerEntity;
        [SerializeField]private bool isInteracting;
         
         public IInteractable CurrentInteractable => currentInteractable;
         public BaseEntity CurrentTarget => currentTarget;
         public bool IsInteracting => isInteracting;
        
         private void Start()
         {
             ownerEntity = GetComponent<BaseEntity>();
             InteractionEvents.OnInteractionEnded += EndInteraction;
         }
         
         public void OnCollisionDetected(ICollisionResult result)
         {
             if (result is EntityCollisionResult entityResult)
             {
                 BaseEntity detectedEntity = entityResult.Entity;
                 if (!isInteracting && (detectedEntity != currentTarget || currentInteractable == null))
                 {
                     IInteractable interactable = detectedEntity.GetComponent<IInteractable>();
                     if (interactable != null)
                     {
                         SetCurrentTarget(detectedEntity, interactable);
                     }
                 }
             }
             else
             {
                 ClearCurrentTarget();
             }
         }
        
         public void TryInteract() => StartInteraction();
        
         private void SetCurrentTarget(BaseEntity target, IInteractable interactable)
         {
             currentTarget = target;
             currentInteractable = interactable;
             InteractionEvents.TriggerInteractionAvailable(interactable);
         }
        
         public void ClearCurrentTarget()
         {
             if (currentTarget != null || currentInteractable != null)
             {
                 if (isInteracting)
                 {
                     EndInteraction();
                 }
                 currentTarget = null;
                 currentInteractable = null;
                 InteractionEvents.TriggerInteractionUnavailable();
             }
         }
        
         private void StartInteraction()
         {
             if (!isInteracting)
             {
                 isInteracting = true;
                 InteractionEvents.TriggerInteractionStarted(currentInteractable, ownerEntity);
             }
         }
        
         private void EndInteraction()
         {
             if (isInteracting)
             {
                 isInteracting = false;
             }
         }
    }
}